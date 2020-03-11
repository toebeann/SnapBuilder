using UnityEngine;

namespace SnapBuilder
{
    internal class ToggleKey
    {
        internal enum Mode
        {
            Press, Hold
        }

        private readonly KeyCode keyCode;
        private readonly Mode keyMode;
        private readonly bool onByDefault;

        private bool enabled;
        public bool Enabled
        {
            get
            {
                switch (keyMode)
                {
                    case Mode.Press:
                        if (Input.GetKeyUp(keyCode))
                        {
                            enabled = !enabled;
                        }
                        break;
                    case Mode.Hold:
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

        public ToggleKey(KeyCode _keyCode, Mode _keyMode, bool _onByDefault)
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
