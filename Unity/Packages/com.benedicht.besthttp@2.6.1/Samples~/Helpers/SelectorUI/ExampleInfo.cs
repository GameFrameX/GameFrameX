using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples.Helpers.SelectorUI
{
    public sealed class ExampleInfo : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Text _header;

        [SerializeField]
        private Text _description;

#pragma warning restore

        private SampleSelectorUI _parentUI;

        private SampleBase _example;

        public void Setup(SampleSelectorUI parentUI, SampleBase example)
        {
            this._parentUI = parentUI;
            this._example = example;

            this._header.text = this._example.name;
            this._description.text = this._example.Description;
        }

        public void OnExecuteExample()
        {
            this._parentUI.ExecuteExample(this._example);
        }
    }
}
