using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Loaders
{
    public class TextureLoader : MonoBehaviour
    {
        public static TextureLoader Instance;

        private static readonly HashSet<string> textureLoadInProgress = new HashSet<string>();

        private static readonly Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();

        private static readonly BlockingCollection<(int, int)> createTexTaskQueue = new BlockingCollection<(int, int)>();
        private static readonly BlockingCollection<Texture2D> createdTexturesQueue = new BlockingCollection<Texture2D>();

        private static readonly BlockingCollection<Action> mainThreadActions = new BlockingCollection<Action>(); 

        public void Awake()
        {
            if (!Instance)
                Instance = this;

            StartCoroutine(nameof(CreateTextureCoroutine));
            StartCoroutine(nameof(DoMainThreadActions));
        }


        public bool GetTextureAndDoAction(string texturePath, Action<Texture2D> action)
        {
            if (loadedTextures.TryGetValue(texturePath, out Texture2D tex))
            {
                action(tex);
                return true; // executed;
            }
            else
            {
                // wait(coroutine) for loaded or load texture
                if (textureLoadInProgress.Contains(texturePath))
                {
                    StartCoroutine(nameof(DoWhenLoaded), (texturePath, action) );
                    return false;
                }
                else
                {
                    // load texture:
                    textureLoadInProgress.Add(texturePath);
                    StartCoroutine(nameof(LoadTextureCoroutine), (texturePath, action, ++DevILThreads));
                    return false; 
                }

            }
        }

        IEnumerator DoWhenLoaded((string, Action<Texture2D> ) loadTextureAction)
        {
            (string texturePath, Action<Texture2D> action) = loadTextureAction;

            yield return new WaitWhile(() => textureLoadInProgress.Contains(texturePath));
            loadedTextures.TryGetValue(texturePath, out Texture2D tex);
            action(tex);
        }

        private static long DevILThreads = 0;
        private static long CurrentDevILThread = 1;

        IEnumerator LoadTextureCoroutine((string, Action<Texture2D>, long) loadTextureActionThread)
        {
            (string texturePath, Action<Texture2D> action, long DevILThread) = loadTextureActionThread;

            yield return new WaitWhile(() => DevILThread > CurrentDevILThread);

            Texture2D tex = null;
            var t = new Thread(() =>
            {
                tex = LoadImageFromData(File.ReadAllBytes(texturePath));
            });
            t.Start();

            yield return new WaitWhile(() => t.IsAlive);

            CurrentDevILThread++;
            loadedTextures.Add(texturePath, tex);
            textureLoadInProgress.Remove(texturePath);
            action(tex);
        }

        IEnumerator CreateTextureCoroutine()
        {
            while (true)
            {
                while (createTexTaskQueue.TryTake(out (int, int) widthHeight , 0))
                {
                    (int width, int height) = widthHeight;
                    createdTexturesQueue.Add(new Texture2D(width, height, TextureFormat.RGBA32, true));
                    yield return null;
                }
                yield return new WaitWhile(() => createTexTaskQueue.Count == 0);
            }
        }

        IEnumerator DoMainThreadActions()
        {
            while (true)
            {
                while (mainThreadActions.TryTake(out Action action, 0))
                {
                    action();
                    yield return null;
                }
                yield return new WaitWhile(() => mainThreadActions.Count == 0);
            }
        }


        private Texture2D LoadImageFromData(byte[] imageData)
        {
            const int cNumImages = 1;
            uint[] handles = new uint[cNumImages];
            Texture2D resTexture = null;

            DevILLoader.ilInit();
            DevILLoader.ilEnable(DevILConstants.IL_ORIGIN_SET);
            DevILLoader.ilGenImages(cNumImages, handles);
            DevILLoader.ilBindImage(handles[0]);

            bool res = DevILLoader.ilLoadL(DevILConstants.IL_TYPE_UNKNOWN, imageData, (uint)imageData.Length);
            if (!res)
            {
                Debug.LogWarning("Error! Cannot load image from data");
                return resTexture;
            }

            int width = DevILLoader.ilGetInteger(DevILConstants.IL_IMAGE_WIDTH); // getting image width
            int height = DevILLoader.ilGetInteger(DevILConstants.IL_IMAGE_HEIGHT); // and height
            //Debug.Log("Base image resolution: w = " + width + "; h = " + height);

            // create result texture
            createTexTaskQueue.Add((width, height));
            while (createdTexturesQueue.Count == 0) ;
            resTexture = createdTexturesQueue.Take();

            Color32[] texColors = GetColorDataFromCurrentImage();

            // set first mip map
            mainThreadActions.Add(() => resTexture.SetPixels32(texColors, 0));

            // other levels of bitmap
            {
                uint currMipMapLevel = 1;
                const uint cMaxMipMapLevel = 15;
                while (currMipMapLevel < cMaxMipMapLevel)
                {
                    res = DevILLoader.ilActiveMipmap(currMipMapLevel);
                    //Debug.Log("res = " + res + " currMipMapLevel = " + currMipMapLevel);
                    if (!res)
                    {
                        break;
                    }

                    texColors = GetColorDataFromCurrentImage();

                    //Debug.Log("currMipMapLevel = " + currMipMapLevel);

                    // set next mip map
                    mainThreadActions.Add(() => resTexture.SetPixels32(texColors, (int)currMipMapLevel));

                    ++currMipMapLevel;

                    // restore base image
                    DevILLoader.ilBindImage(handles[0]);
                }
            }

            mainThreadActions.Add(() => resTexture.Apply(true));
            mainThreadActions.Add(() => resTexture.Compress(false)); // !!! if you don't want to trash RAM

            DevILLoader.ilDeleteImages(cNumImages, handles);

            return resTexture;
        }

        private Color32[] GetColorDataFromCurrentImage()
        {
            int width = DevILLoader.ilGetInteger(DevILConstants.IL_IMAGE_WIDTH); // getting image width
            int height = DevILLoader.ilGetInteger(DevILConstants.IL_IMAGE_HEIGHT); // and height

            //Debug.Log("Image resolution: w = " + width + "; h = " + height);

            /* how much memory will we need? */
            int memoryNeeded = width * height * 4;

            /* We multiply by 4 here because we want 4 components per pixel */
            byte[] imageColorData = new byte[memoryNeeded];

            /* finally get the image data */
            DevILLoader.ilCopyPixels(0, 0, 0, (uint)width, (uint)height, 1, DevILConstants.IL_RGBA, DevILConstants.IL_UNSIGNED_BYTE, imageColorData);

            if (imageColorData.Length <= 0)
            {
                return null;
            }

            // create colors from color data
            Color32[] texColors = new Color32[imageColorData.Length / 4];

            for (int i = 0, j = 0; i < imageColorData.Length; i += 4, ++j)
            {
                texColors[j].r = imageColorData[i];
                texColors[j].g = imageColorData[i + 1];
                texColors[j].b = imageColorData[i + 2];
                texColors[j].a = imageColorData[i + 3];
            }

            return texColors;
        }




    }
}
