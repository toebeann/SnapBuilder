using UnityEngine;

namespace SnapBuilder
{
    internal class ToggleKey
    {
        private readonly KeyCode keyCode;
        private readonly KeyMode keyMode;
        private readonly bool onByDefault;

        private bool enabled;
        public bool Enabled
        {
            get
            {
                switch (keyMode)
                {
                    case KeyMode.Press:
                        if (Input.GetKeyUp(keyCode))
                        {
                            enabled = !enabled;
                        }
                        break;
                    case KeyMode.Hold:
                        if (Input.GetKeyDown(keyCode))
                        {
                            enabled = !onByDefault;
                        }
                        else if (Input.GetKeyUp(keyCode))
                        {
                            enabled = onByDefault;
                        }
                        break;
                }
                return enabled;
            }
        }

        public ToggleKey(KeyCode _keyCode, KeyMode _keyMode, bool _onByDefault)
        {
            keyCode = _keyCode;
            keyMode = _keyMode;
            onByDefault = _onByDefault;
            enabled = onByDefault;
        }

        public void Reset()
        {
            enabled = onByDefault;
        }
    }
}
