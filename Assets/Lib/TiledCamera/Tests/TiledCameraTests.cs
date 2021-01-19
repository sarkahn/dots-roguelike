using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Sark.RenderUtils
{
    public class TiledCameraTests
    {
        [Test]
        public void CreateFromCode()
        {
            var go = new GameObject("TiledCamera", typeof(TiledCamera));
            var cam = go.GetComponent<TiledCamera>();

            Assert.IsNotNull(cam.PixelCamera);
            Assert.AreEqual(typeof(PixelPerfectCamera), cam.PixelCamera.GetType());
            Assert.IsNotNull(cam.ClearCamera);

            Assert.AreEqual(CameraClearFlags.SolidColor, cam.ClearCamera.clearFlags);
        }
    }
}