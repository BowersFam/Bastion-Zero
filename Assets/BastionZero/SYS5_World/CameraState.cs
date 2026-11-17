namespace BastionZero.World
{
    /// <summary>
    /// SYS5 §10.1 camera state list. Names are the spec's own, so a readout can be traced
    /// straight back to the document.
    /// </summary>
    public enum CameraState
    {
        /// <summary>Default framing: dead zone, trailing, look-ahead (WC-W1 … WC-W11).</summary>
        C_FOLLOW = 0,

        /// <summary>An authored framing volume locks an axis or sets a fixed anchor.</summary>
        C_ZONE = 1,

        /// <summary>Manual look: hold up or down while grounded and idle, pans 4 tiles.</summary>
        C_LOOK = 2,

        /// <summary>Arena lock: a fixed frame over the arena until the fight ends.</summary>
        C_ARENA = 3,

        /// <summary>Instant reposition. One frame, then back to C_FOLLOW.</summary>
        C_CUT = 4,

        /// <summary>Death hold: the camera is frozen where it was.</summary>
        C_HOLD = 5,
    }
}
