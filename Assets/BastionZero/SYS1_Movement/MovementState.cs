namespace BastionZero.Movement
{
    /// <summary>
    /// The ten movement states of the vertical slice (13-vertical-slice §5.1, owner SYS1 §2).
    /// Names are the spec's, so a state readout can be traced straight back to the document.
    /// Deferred and therefore absent: G-CROUCH, A-FASTFALL, A-GLIDE, X-GRAPPLE.
    /// </summary>
    public enum MovementState
    {
        /// <summary>Grounded, no horizontal speed.</summary>
        G_IDLE = 0,

        /// <summary>Grounded, moving.</summary>
        G_RUN = 1,

        /// <summary>Rising with Jump held inside MV-08 max hold time.</summary>
        A_RISE_HELD = 2,

        /// <summary>Rising with Jump released or the hold window spent.</summary>
        A_RISE_FREE = 3,

        /// <summary>Inside the MV-14 apex band, where gravity is multiplied by MV-15.</summary>
        A_APEX = 4,

        /// <summary>Descending.</summary>
        A_FALL = 5,

        /// <summary>Holding a cling-able wall (PE-U1).</summary>
        W_CLING = 6,

        /// <summary>Rising out of a wall jump.</summary>
        W_JUMP = 7,

        /// <summary>Knockback after damage; input ignored. Driven by SYS2.</summary>
        H_KNOCK = 8,

        /// <summary>Warp after a pit or warp hazard. Driven by SYS2.</summary>
        H_WARP = 9,
    }
}
