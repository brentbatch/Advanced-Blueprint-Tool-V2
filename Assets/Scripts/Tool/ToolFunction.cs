using System;
using UnityEngine;

namespace Assets.Scripts.Tool
{
    public class ToolFunction
    {
        public string title;
        public string description;
        public Sprite sprite;
        public Action OnUpdate;
        public Action OnFixedUpdate;
        /// <summary>
        /// happens when switching to a new function, 
        /// abstracttool also makes it trigger when equipping tool
        /// </summary>
        public Action OnEquip;
        /// <summary>
        /// happens when switching to a new function, 
        /// abstracttool also makes it trigger when unequipping tool
        /// </summary>
        public Action OnUnEquip;
        public Action<bool> OnLeftClick;
        public Action<bool> OnRightClick;
        public Action<bool> OnFunctionInfo;
        public Action<bool> OnInteract;
        public Action<Vector3> OnMove2;
        public Action<bool> OnNextRotation;
        public Action<bool> OnPreviousRotation;
        public Action<bool> OnR;
        public Action<bool> OnF;
        public Action<bool> OnTab;
        public Func<bool, bool> OnEsc;
        public Func<bool> OnFocus;
    }
}