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

        public Vector3 rotatedBounds;
        public int xaxis;
        public int zaxis;

        public void SetColor(string strColor)
        {
            strColor = strColor.First() == '#' ? strColor : '#' + strColor;
            if (ColorUtility.TryParseHtmlString(strColor, out Color color))
            {
                foreach (var meshRend in gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRend.material.color = color;
                }
            }
        }


        public void SetBlueprintPosition(Pos pos) // y and z reversed
        {
            var transform = gameObject.transform;
            var position = transform.position;
            (position.x, position.y, position.z) = (pos.X, pos.Z, pos.Y);
            transform.position = position;
        }

        public void SetBlueprintBounds(Data.Bounds bounds) // y and z reversed
        {
            if (shape is Part) // only blocks have bounds.
            {
                Debug.LogWarning($"Trying to set bounds on a child that is a part!");
                return;
            }
            var childMesh = gameObject.transform.GetChild(0);
            var scale = childMesh.transform.localScale;

            (scale.x, scale.y, scale.z) = (bounds.X, bounds.Z, bounds.Y);

            childMesh.transform.localScale = scale;
            childMesh.transform.localPosition = scale / 2;
        }

        /// <summary>
        /// use rotated bounds for features.
        /// </summary>
        private void CalculateRotatedBounds()
        {
            //throw new NotImplementedException();
        }

        public void SetBlueprintRotation(int xaxis, int zaxis)
        {
            this.xaxis = xaxis;
            this.zaxis = zaxis;

            bool xpos = xaxis > 0;
            bool zpos = zaxis > 0;
            /*
            int rotatex, rotatez;
            switch (Math.Abs(xaxis))
            {
                case 1:
                    baseLinkRotatex.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, xpos ? 0 : 1), 180);
                    baseLinkMove.Children.Add(baseLinkRotatex);
                    switch (Math.Abs(zaxis))
                    {
                        case 1:
                            MessageBox.Show("Incorrect rotationset found !");
                            break;
                        case 2:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(zpos ? -1 : 1, 0, 0), 90);
                            break;
                        case 3:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(zpos ? 0 : 1, 0, 0), 180);
                            break;
                    }
                    baseLinkMove.Children.Add(baseLinkRotatez);
                    break;
                case 2:
                    baseLinkRotatex.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, xpos ? 1 : -1), 90);
                    baseLinkMove.Children.Add(baseLinkRotatex);
                    switch (Math.Abs(zaxis))
                    {
                        case 1:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, zpos ? 1 : -1, 0), 90);
                            break;
                        case 2:
                            MessageBox.Show("Incorrect rotationset found !");
                            break;
                        case 3:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, zpos ? 0 : 1, 0), 180);
                            break;
                    }
                    baseLinkMove.Children.Add(baseLinkRotatez);
                    break;
                case 3:
                    baseLinkRotatex.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, xpos ? -1 : 1, 0), 90);
                    baseLinkMove.Children.Add(baseLinkRotatex);
                    switch (Math.Abs(zaxis))
                    {
                        case 1:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, (zpos == xpos) ? 1 : 0), 180);
                            break;
                        case 2:
                            baseLinkRotatez.Rotation = new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, (zpos == xpos) ? -1 : 1), 90);
                            break;
                        case 3:
                            MessageBox.Show("Incorrect rotationset found !");
                            break;
                    }
                    baseLinkMove.Children.Add(baseLinkRotatez);
                    break;
            } //rotations translate!
            //*/
            CalculateRotatedBounds();
        }

        public void Rotate()
        {

        }

    }
}
