/*
 * PlayFab Unity SDK
 *
 * Copyright (c) Microsoft Corporation
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

namespace PlayFab.Multiplayer.InteropWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PFMultiplayerPort
    {
        public PFMultiplayerPort(string name, uint num, PFMultiplayerProtocolType protocol)
        {
            this.Name = name;
            this.Num = num;
            this.Protocol = protocol;
        }

        internal unsafe PFMultiplayerPort(Interop.PFMultiplayerPort* interopStruct)
        {
            Name = Converters.PtrToStringUTF8((IntPtr)interopStruct->name);
            Num = interopStruct->num;
            Protocol = (PFMultiplayerProtocolType)(interopStruct->protocol);
        }

        internal unsafe Interop.PFMultiplayerPort* ToPointer(DisposableCollection disposableCollection)
        {
            Interop.PFMultiplayerPort portPtr = new Interop.PFMultiplayerPort();

            UTF8StringPtr namePtr = new UTF8StringPtr(this.Name, disposableCollection);
            portPtr.name = namePtr.Pointer;

            portPtr.num = this.Num;
            portPtr.protocol = (Interop.PFMultiplayerProtocolType)this.Protocol;

            return (Interop.PFMultiplayerPort*)Converters.StructToPtr<Interop.PFMultiplayerPort>(portPtr, disposableCollection);

        }

        public string Name { get; set; }

        public uint Num { get; set; }

        public PFMultiplayerProtocolType Protocol { get; set; }
    }
}
