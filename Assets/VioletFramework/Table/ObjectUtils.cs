using System;
using UnityEngine;

namespace FunEngine.Utils
{

    public class ObjectUtils
    {

        // ------------------------------------------------------------------------------
        // 创建对象实例
        // ------------------------------------------------------------------------------
        public static object CreateInstance (string className)
        {
            //首先获取当前Assembly中的类
            try
            {
                return Activator.CreateInstance (Type.GetType (className, true));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            //
            string[] assemblies = new string[]
            {
                "Assembly-CSharp-Editor",
                "Assembly-CSharp-firstpass",
                "Assembly-CSharp"
            };
            foreach (string assembly in assemblies)
            {
                try
                {
                    object raw = Activator.CreateInstance(assembly, className);
                    if (raw is System.Runtime.Remoting.ObjectHandle)
                    {
                        return ((System.Runtime.Remoting.ObjectHandle)raw).Unwrap();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            return null;
        }

        // ------------------------------------------------------------------------------
        // 获取对象类名
        // ------------------------------------------------------------------------------
        public static string GetClassName (object obj, bool needPackage)
        {
            Type t = obj.GetType ();
            return needPackage ? t.FullName : t.Name;
        }

        // ------------------------------------------------------------------------------
        // 获取对象类名
        // ------------------------------------------------------------------------------
        public static void DisposeObject(object obj)
        {
            if (obj is GameObject)
            {
                GameObject.Destroy(obj as GameObject);
            }
            else if (obj is Component)
            {
                GameObject.Destroy(obj as Component);
            }
            else if (obj is AssetBundle)
            {
                ((AssetBundle)obj).Unload(false);
            }
            else if (obj is IDisposable)
            {
                ((IDisposable)obj).Dispose();
            }
            else if (obj is Delegate)
            {
                Delegate.RemoveAll((Delegate)obj, (Delegate)obj);
            }
        }
    }
}

