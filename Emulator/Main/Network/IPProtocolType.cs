//
// Copyright (c) Antmicro
// Copyright (c) Realtime Embedded
//
// This file is part of the Emul8 project.
// Full license details are defined in the 'LICENSE' file.
//

namespace Emul8.Network
{
    public enum IPProtocolType : byte
    {
        HOPOPTS = 0,
        ICMP = 1,
        IGMP = 2,
        GGP = 3,
        IPV4 = 4,
        ST = 5,
        TCP = 6,
        EGP = 8,
        PUP = 12,
        UDP = 17,
        IDP = 22,
        TP = 29,
        IPV6 = 41,
        ROUTING = 43,
        FRAGMENT = 44,
        RSVP = 46,
        GRE = 47,
        ESP = 50,
        AH = 51,
        ICMPV6 = 58,
        NONE = 59,
        DSTOPTS = 60,
        MTP = 92,
        ENCAP = 98,
        PIM = 103,
        COMP = 108
    }
}
