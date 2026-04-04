using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace BlackStartX.GestureManager
{
    public class CurrentModel
    {
        public static event Action OnAvatarSwitch;
        public static GameObject ModelGO { get; private set; }
        public static Transform ModelRoot { get; private set; }

        static readonly float avatarScanInterval = 0.25f;
        static float nextAvatarScan;

        public static void OnAwake()
        {
            var modelRootGO = GameObject.Find("Model");
            if (modelRootGO != null)
                ModelRoot = modelRootGO.transform;
        }
        public static void OnUpdate()
        {
            if (Time.unscaledTime >= nextAvatarScan)
            {
                UpdateCurrentAvatar();
                nextAvatarScan = Time.unscaledTime + avatarScanInterval;
            }
        }
        static void UpdateCurrentAvatar()
        {
            if (!ModelRoot) return;

            for (int i = 0; i < ModelRoot.childCount; i++)
            {
                var child = ModelRoot.GetChild(i).gameObject;
                if (!child.activeInHierarchy) continue;
                if (ModelGO == child) return;
                ModelGO = child;

                UpdateAvatarComponents();
                OnAvatarSwitch?.Invoke();

                return;
            }
        }
        static void UpdateAvatarComponents()
        {
            AvatarBigScreenHandlerProxy.Inst = GetComponent<AvatarBigScreenHandler>();
        }
        public static T GetComponent<T>() where T : Component
            => ModelGO.GetComponent<T>();


        public static class AvatarBigScreenHandlerProxy
        {
            public static AvatarBigScreenHandler Inst;

            static readonly Func<AvatarBigScreenHandler, bool> _isBigScreenActive = MakeGetter<AvatarBigScreenHandler, bool>("isBigScreenActive");
            public static bool isBigScreenActive
            {
                get => _isBigScreenActive(Inst);
            }
        }

        static Func<TInstance, TField> MakeGetter<TInstance, TField>(string fieldName)
        {
            var fieldInfo = typeof(TInstance).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var instanceParam = Expression.Parameter(typeof(TInstance));
            MemberExpression fieldAccess;
            if (fieldInfo.IsStatic)
                fieldAccess = Expression.Field(null, fieldInfo);
            else
                fieldAccess = Expression.Field(instanceParam, fieldInfo);

            var lambda = Expression.Lambda<Func<TInstance, TField>>(fieldAccess, instanceParam);

            return lambda.Compile();
        }

    }
}