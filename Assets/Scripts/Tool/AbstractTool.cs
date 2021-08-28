using Assets.Scripts.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Tool
{
    public abstract class AbstractTool
    {
        protected static readonly PlayerController PlayerController = GameController.Instance.playerController;
        protected static readonly TMP_Text TitleText = GameController.Instance.hotbarTitleText;
        protected static readonly TMP_Text DescriptionText = GameController.Instance.hotbarDescriptionText;

        protected readonly string title;
        protected readonly string description;
        public Sprite sprite;

        public List<ToolFunction> functions;
        public ToolFunction selectedToolFunction;
        public int SelectedToolIndex { get; private set; }

        protected Vector3 previousArrowsState;
        protected bool leftClickButtonState;
        protected bool rightClickButtonState;
        protected bool interactButtonState;
        protected bool nextRotationButtonState;
        protected bool previousRotationButtonState;
        protected bool rButtonState;
        protected bool fButtonState;
        protected bool tabButtonState;
        protected bool escButtonState;

        public AbstractTool(string title = "unnamed tool", string description = "")
        {
            this.title = title;
            this.description = description;
            DescriptionText.enabled = false;

            this.SelectedToolIndex = 0;
        }

        public virtual void SelectToolFunction(int index)
        {
            if (index >= 0 && index < functions.Count)
            {
                selectedToolFunction?.OnUnEquip?.Invoke();
                selectedToolFunction = functions[index];
                SelectedToolIndex = index;
                ShowToolInfo();
                selectedToolFunction.OnEquip?.Invoke();
                if (DescriptionText.enabled)
                {
                    this.OnToolInfo(true);
                }
            }
            else
            {
                selectedToolFunction?.OnUnEquip?.Invoke();
                selectedToolFunction = null;
                SelectedToolIndex = -1;
            }
        }

        /// <summary>
        /// called after playercontroller.OnStart()
        /// </summary>
        public virtual void OnStart()
        {

        }

        /// <summary>
        /// default implementation: toolfunction.onEquip
        /// </summary>
        public virtual void OnEquip()
        {
            ShowToolInfo();
            TitleText.enabled = true;
            DescriptionText.enabled = false;
        }
        /// <summary>
        /// default implementation: toolfunction.onUnEquip
        /// </summary>
        public virtual void OnUnEquip()
        {
            ShowToolInfo();
            TitleText.enabled = true;
            DescriptionText.enabled = false;
        }
        /// <summary>
        /// default implementation: toolfunction.onUpdate
        /// </summary>
        public virtual void OnUpdate()
        {
            selectedToolFunction?.OnUpdate?.Invoke();
        }
        /// <summary>
        /// default implementation: toolfunction.onFixedUpdate
        /// </summary>
        public virtual void OnFixedUpdate()
        {
            selectedToolFunction?.OnFixedUpdate?.Invoke();
        }

        /// <summary>
        /// on left click when tool equipped.
        /// basic implementation: save state in leftClickState & perform selectedToolFunction.onLeftClick
        /// </summary>
        /// <param name="isKeyDown"></param>
        public virtual void OnLeftClick(bool isKeyDown)
        {
            //if (HotbarController.IsToolBarSelected)
            //    return;
            leftClickButtonState = isKeyDown;
            selectedToolFunction?.OnLeftClick?.Invoke(isKeyDown);
        }
        /// <summary>
        /// on left click when tool equipped.
        /// basic implementation: save state in rightClickState & perform selectedToolFunction.onRightClick
        /// </summary>
        /// <param name="isKeyDown"></param>
        public virtual void OnRightClick(bool isKeyDown)
        {
            //if (HotbarController.IsToolBarSelected)
            //    return;
            rightClickButtonState = isKeyDown;
            selectedToolFunction?.OnRightClick?.Invoke(isKeyDown);
        }

        public virtual void OnInteract(bool isKeyDown)
        {
            //if (HotbarController.IsToolBarSelected)
            //    return;
            interactButtonState = isKeyDown;
            selectedToolFunction?.OnInteract?.Invoke(isKeyDown);
        }

        public virtual void OnNextRotation(bool isKeyDown)
        {
            //if (HotbarController.IsToolBarSelected)
            //    return;
            nextRotationButtonState = isKeyDown;
            selectedToolFunction?.OnNextRotation?.Invoke(isKeyDown);
        }

        public virtual void OnPreviousRotation(bool isKeyDown)
        {
            //if (HotbarController.IsToolBarSelected)
            //    return;
            previousRotationButtonState = isKeyDown;
            selectedToolFunction?.OnPreviousRotation?.Invoke(isKeyDown);
        }


        public virtual void OnMove2(Vector3 vector3)
        {
            previousArrowsState = vector3;
            selectedToolFunction?.OnMove2?.Invoke(vector3);
        }
        public virtual void OnR(bool isKeyDown)
        {
            //if (HotbarController.IsToolBarSelected)
            //    return;
            rButtonState = isKeyDown;
            selectedToolFunction?.OnR?.Invoke(isKeyDown);
        }
        public virtual void OnF(bool isKeyDown)
        {
            //if (HotbarController.IsToolBarSelected)
            //    return;
            fButtonState = isKeyDown;
            selectedToolFunction?.OnF?.Invoke(isKeyDown);
        }
        public virtual void OnTab(bool isKeyDown)
        {
            //if (HotbarController.IsToolBarSelected)
            //    return;
            tabButtonState = isKeyDown;
            selectedToolFunction?.OnTab?.Invoke(isKeyDown);
        }
        /// <summary>
        /// This method is meant to close menu's on 'esc'
        /// </summary>
        /// <param name="isKeyDown"></param>
        /// <returns>return true to open escape menu</returns>
        public virtual bool OnEsc(bool isKeyDown)
        {
            escButtonState = isKeyDown;
            return selectedToolFunction?.OnEsc == null || selectedToolFunction.OnEsc.Invoke(isKeyDown);
        }
        /// <summary>
        /// Similar to 'esc', called when clicking next to ui, on the background
        /// this is also meant for closing menu's
        /// </summary>
        /// <param name="isKeyDown"></param>
        /// <returns>return true to capture cursor</returns>
        public virtual bool OnFocus()
        {
            return selectedToolFunction?.OnFocus == null || selectedToolFunction.OnFocus.Invoke();
        }

        /// <summary>
        /// on clicking an ACTIVE function in the hotbar
        /// default implementation: onInteract(true) oninteract(false)
        /// </summary>
        public virtual void OnHotBarFunctionClick()
        {
            selectedToolFunction?.OnInteract?.Invoke(true);
            selectedToolFunction?.OnInteract?.Invoke(false);
        }

        /// <summary>
        /// on clicking 'H' (help)
        /// basic implementation : 
        /// show title/description of tool/function in the title/description panel of the hotbar.
        /// in case the toolfunction contains onFunctionInfo, this will be called
        /// </summary>
        public virtual void OnToolInfo(bool isKeyDown)
        {
            if (selectedToolFunction?.OnFunctionInfo != null)
            {
                selectedToolFunction?.OnFunctionInfo.Invoke(isKeyDown);
                return;
            }
            ShowToolInfo();
            // toggle:
            TitleText.enabled = !isKeyDown;
            DescriptionText.enabled = isKeyDown;
            //GameController.Instance.StartCoroutine(this.ResetToolInfo);
        }

        public virtual void ShowToolInfo()
        {
            string title = HotbarController.IsToolBarSelected ? this.title : selectedToolFunction?.title;
            string description = HotbarController.IsToolBarSelected ? this.description : selectedToolFunction?.description;
            TitleText.text = title;
            DescriptionText.text = description;
        }

        IEnumerator ResetToolInfo()
        {
            yield return new WaitForSeconds(2f);
            //DescriptionText.text = "";
        }
    }
}