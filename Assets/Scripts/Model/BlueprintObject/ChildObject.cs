using Assets.Scripts.Loaders;
using Assets.Scripts.Model.Data;
using Assets.Scripts.Model.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Model.BlueprintObject
{
    public class ChildObject : MonoBehaviour
    {
        public Shape shape;

        public void SetRotation(int xaxis, int zaxis)
        {

        }

        public void SetColor(string strColor)
        {
            strColor = strColor.First() == '#' ? strColor : '#' + strColor;
            if (ColorUtility.TryParseHtmlString(strColor, out Color color))
            {
                // move this to Shape class (ColorShape())
                if (shape is Block)
                {
                    gameObject.GetComponent<MeshRenderer>().material.color = color;
                }
                else
                {
                    foreach (var meshRend in gameObject.GetComponentsInChildren<MeshRenderer>())
                    {
                        meshRend.material.color = color;
                    }
                }
            }
        }


        public void SetBlueprintPosition(Pos pos)
        {
            var transform = gameObject.transform;
            var position = transform.position;
            position.x = pos.X;
            position.y = pos.Z;
            position.z = pos.Y;
            transform.position = position;
        }

        public void SetBlueprintBounds(Data.Bounds bounds)
        {
            var scale = transform.localScale;
            scale.x = bounds.X;
            scale.y = bounds.Z;
            scale.z = bounds.Y;
            transform.localScale = scale;
        }

        public void SetBlueprintRotation(int xaxis, int zaxis)
        {
            throw new NotImplementedException();
        }
    }
}
