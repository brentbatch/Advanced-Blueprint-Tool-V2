using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Plugins.ColorGradientPicker.Scripts
{
    public class ColorSaver : MonoBehaviour
    {
        [SerializeField] private GameObject choiceObject;
        [SerializeField] private GameObject chosenSlot;

        [SerializeField] private RawImage colorComponent;

        [SerializeField] private ColorPicker colorPicker;

        public void ChooseSlot(GameObject slot)
        {
            choiceObject.transform.SetParent(slot.transform, false);
            chosenSlot = slot;
        }

        public void SaveColorPickerColor()
        {
            chosenSlot.GetComponent<Image>().color = colorComponent.color;
        }

        public void UseSavedColor()
        {
            colorPicker.SetHexa(ColorUtility.ToHtmlStringRGB(chosenSlot.GetComponent<Image>().color));
        }
    }
}
