using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Normal.Realtime;
using TMPro;

/*
 * Rainfall.XRCore.Avatar * 
 * Component to react on changes of AvatarAppearanceModel
 */
namespace Normal.Realtime
{
    public class AvatarAppearance : RealtimeComponent
    {
        public TextMeshPro _text;
        public GameObject  _laserpointer;
        private AvatarAppearanceModel _model;

        private AvatarAppearanceModel model
        {
            set
            {
                if (_model != null)
                {
                    _model.nameDidChange    -= NameDidChange;
                    _model.pointerDidChange -= PointerDidChange;
                }

                // Store the model
                _model = value;

                if (_model != null)
                {
                    UpdateAvatarName();
                    UpdateAvatarPointer();

                    _model.nameDidChange    += NameDidChange;
                    _model.pointerDidChange += PointerDidChange;
                }
            }
        }

        private void NameDidChange(AvatarAppearanceModel model, string value)
        {
            UpdateAvatarName();
        }

        private void PointerDidChange(AvatarAppearanceModel model, bool value)
        {
            UpdateAvatarPointer();
        }

        public void UpdateAvatarName()
        {
            _text.text = _model.name;
        }

        public void UpdateAvatarPointer()
        {
            _laserpointer.SetActive(_model.pointer);
        }

        public void SetName(string name)
        {
            _model.name = name;
        }

        public void SetPointer(bool pointer)
        {
            _model.pointer = pointer;
        }
    }
}