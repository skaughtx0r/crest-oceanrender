﻿// Crest Ocean System

// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Crest
{
    public interface ILodDataInput
    {
        void Draw(CommandBuffer buf, float weight, int isTransition, int lodIdx);
        float Wavelength { get; }
        bool Enabled { get; }
    }

    /// <summary>
    /// Base class for scripts that register input to the various LOD data types.
    /// </summary>
    public abstract class RegisterLodDataInputBase : MonoBehaviour, ILodDataInput
    {
        public abstract float Wavelength { get; }

        public abstract bool Enabled { get; }

        public static int sp_Weight = Shader.PropertyToID("_Weight");

        static Dictionary<System.Type, List<ILodDataInput>> _registrar = new Dictionary<System.Type, List<ILodDataInput>>();

        public static List<ILodDataInput> GetRegistrar(System.Type lodDataMgrType)
        {
            List<ILodDataInput> registered;
            if (!_registrar.TryGetValue(lodDataMgrType, out registered))
            {
                registered = new List<ILodDataInput>();
                _registrar.Add(lodDataMgrType, registered);
            }
            return registered;
        }

        Renderer _renderer;
        Material[] _materials = new Material[2];

        protected virtual void Start()
        {
            _renderer = GetComponent<Renderer>();

            if (_renderer)
            {
                _materials[0] = _renderer.sharedMaterial;
                _materials[1] = new Material(_renderer.sharedMaterial);
            }
        }

        public void Draw(CommandBuffer buf, float weight, int isTransition, int lodIdx)
        {
            if (_renderer && weight > 0f)
            {
                _materials[isTransition].SetFloat(sp_Weight, weight);
                _materials[isTransition].SetInt(LodDataMgr.sp_LD_SliceIndex, lodIdx);

                buf.DrawRenderer(_renderer, _materials[isTransition]);
            }
        }

        public int MaterialCount => _materials.Length;
        public Material GetMaterial(int index) => _materials[index];

#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        static void InitStatics()
        {
            // Init here from 2019.3 onwards
            _registrar = new Dictionary<System.Type, List<ILodDataInput>>();
            sp_Weight = Shader.PropertyToID("_Weight");
        }
    }

    /// <summary>
    /// Registers input to a particular LOD data.
    /// </summary>
    public abstract class RegisterLodDataInput<LodDataType> : RegisterLodDataInputBase
        where LodDataType : LodDataMgr
    {
        [SerializeField] bool _disableRenderer = true;

        protected abstract Color GizmoColor { get; }

        protected virtual void OnEnable()
        {
            if (_disableRenderer)
            {
                var rend = GetComponent<Renderer>();
                if (rend)
                {
                    rend.enabled = false;
                }
            }

            var registrar = GetRegistrar(typeof(LodDataType));
            registrar.Add(this);
        }

        protected virtual void OnDisable()
        {
            var registered = GetRegistrar(typeof(LodDataType));
            if (registered != null)
            {
                registered.Remove(this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var mf = GetComponent<MeshFilter>();
            if (mf)
            {
                Gizmos.color = GizmoColor;
                Gizmos.DrawWireMesh(mf.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
            }
        }
    }
}
