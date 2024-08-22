using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System;
using UnityEngine.UIElements;

namespace EditorExtension
{
    public class NetWindow : EditorWindow
    {
        NetView netView;
        public static NetWindow OpenNet(INetProvider provider)
        {
            var net = EditorWindow.CreateInstance<NetWindow>();
            net.titleContent = new GUIContent();
            if (string.IsNullOrEmpty(provider.NetTitle))
            {
                net.titleContent.text = "新网络";
            }
            else
            {
                net.titleContent.text = provider.NetTitle;
            }
            net.InitNet(provider);
            net.Show();
            return net;
        }
        void InitNet(INetProvider provider)
        {
            netView = new NetView();
            netView.netWindow = this;
            this.rootVisualElement.Add(netView);
            netView.Initialize(provider);
        }
        private void OnFocus()
        {
            if (NetView.instance != netView)
            {
                NetView.instance = netView;
            }
        }
        private void OnDisable()
        {
            try
            {
                netView.Dispose();
                this.rootVisualElement.Remove(netView);
            }
            catch (System.Exception)
            {
                return;
            }
        }
    }

    public interface INetProvider
    {
        string NetTitle { get; }
        HashSet<INodeContent> NodeContents { get; }
        INodeContent ProvideRootContent();
        NodeContentType[] ProvideNodeContentTypes();
        PortConnectionMatrix ProvidePortConnectionMatrix();
    }
}

