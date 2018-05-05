﻿using UnityEngine;

namespace Crest
{
    public class RenderOceanDepth : MonoBehaviour
    {
        void Start()
        {
            RefreshOceanDepthRenderers();
        }

        void OnDisable()
        {
            RefreshOceanDepthRenderers();
        }

        void RefreshOceanDepthRenderers()
        {
            if (OceanRenderer.Instance == null || OceanRenderer.Instance.Builder == null)
                return;

            // notify WDCs that there is a new contributor to ocean depth
            foreach (var cam in OceanRenderer.Instance.Builder._shapeCameras)
            {
                var wdc = cam.GetComponent<WaveDataCam>();
                if (wdc)
                {
                    wdc.OnOceanDepthRenderersChanged();
                }
            }
        }
    }
}