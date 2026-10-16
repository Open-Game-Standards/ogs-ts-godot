#pragma warning disable 8600,8601,8602,8603,8604,8618,8619,8625,8629,8632
using Capnp;
using Capnp.Rpc;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CapnpGen
{
    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xba8a07aef400a3afUL)]
    public class Vector3 : ICapnpSerializable
    {
        public const UInt64 typeId = 0xba8a07aef400a3afUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            X = reader.X;
            Y = reader.Y;
            Z = reader.Z;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.X = X;
            writer.Y = Y;
            writer.Z = Z;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float X
        {
            get;
            set;
        }

        public float Y
        {
            get;
            set;
        }

        public float Z
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public float X => ctx.ReadDataFloat(0UL, 0F);
            public float Y => ctx.ReadDataFloat(32UL, 0F);
            public float Z => ctx.ReadDataFloat(64UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public float X
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float Y
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float Z
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x83acd7abe1f662d4UL)]
    public class Quaternion : ICapnpSerializable
    {
        public const UInt64 typeId = 0x83acd7abe1f662d4UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            X = reader.X;
            Y = reader.Y;
            Z = reader.Z;
            W = reader.W;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.X = X;
            writer.Y = Y;
            writer.Z = Z;
            writer.W = W;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float X
        {
            get;
            set;
        }

        public float Y
        {
            get;
            set;
        }

        public float Z
        {
            get;
            set;
        }

        public float W
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public float X => ctx.ReadDataFloat(0UL, 0F);
            public float Y => ctx.ReadDataFloat(32UL, 0F);
            public float Z => ctx.ReadDataFloat(64UL, 0F);
            public float W => ctx.ReadDataFloat(96UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public float X
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float Y
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float Z
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public float W
            {
                get => this.ReadDataFloat(96UL, 0F);
                set => this.WriteData(96UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc813c07308ec1352UL)]
    public class Range : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc813c07308ec1352UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Min = reader.Min;
            Max = reader.Max;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Min = Min;
            writer.Max = Max;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public double Min
        {
            get;
            set;
        }

        public double Max
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public double Min => ctx.ReadDataDouble(0UL, 0);
            public double Max => ctx.ReadDataDouble(64UL, 0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public double Min
            {
                get => this.ReadDataDouble(0UL, 0);
                set => this.WriteData(0UL, value, 0);
            }

            public double Max
            {
                get => this.ReadDataDouble(64UL, 0);
                set => this.WriteData(64UL, value, 0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8630a811f6bd1e18UL)]
    public enum ObjectType : ushort
    {
        other,
        vehicle,
        aircraft,
        watercraft,
        spacecraft,
        humanoid,
        camera,
        creature
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc4d8aa04fa065ed7UL)]
    public enum ContactMedium : ushort
    {
        other,
        air,
        asphalt,
        dirt,
        grass,
        gravel,
        rumbleStrip,
        water,
        snow,
        ice,
        vacuum
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x97648f18e2281faeUL)]
    public class Environment : ICapnpSerializable
    {
        public const UInt64 typeId = 0x97648f18e2281faeUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            AirDensity = reader.AirDensity;
            Temperature = reader.Temperature;
            Pressure = reader.Pressure;
            Gravity = reader.Gravity;
            Medium = reader.Medium;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.AirDensity = AirDensity;
            writer.Temperature = Temperature;
            writer.Pressure = Pressure;
            writer.Gravity = Gravity;
            writer.Medium = Medium;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float AirDensity
        {
            get;
            set;
        }

        public float Temperature
        {
            get;
            set;
        }

        public float Pressure
        {
            get;
            set;
        }

        public float Gravity
        {
            get;
            set;
        }

        public CapnpGen.ContactMedium Medium
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public float AirDensity => ctx.ReadDataFloat(0UL, 0F);
            public float Temperature => ctx.ReadDataFloat(32UL, 0F);
            public float Pressure => ctx.ReadDataFloat(64UL, 0F);
            public float Gravity => ctx.ReadDataFloat(96UL, 0F);
            public CapnpGen.ContactMedium Medium => (CapnpGen.ContactMedium)ctx.ReadDataUShort(128UL, (ushort)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 0);
            }

            public float AirDensity
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float Temperature
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float Pressure
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public float Gravity
            {
                get => this.ReadDataFloat(96UL, 0F);
                set => this.WriteData(96UL, value, 0F);
            }

            public CapnpGen.ContactMedium Medium
            {
                get => (CapnpGen.ContactMedium)this.ReadDataUShort(128UL, (ushort)0);
                set => this.WriteData(128UL, (ushort)value, (ushort)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xefed195db09682ddUL)]
    public class DiscoveryFrame : ICapnpSerializable
    {
        public const UInt64 typeId = 0xefed195db09682ddUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            SchemaVersion = reader.SchemaVersion;
            GameName = reader.GameName;
            SessionStartUnixUs = reader.SessionStartUnixUs;
            Environment = CapnpSerializable.Create<CapnpGen.Environment>(reader.Environment);
            Objects = reader.Objects?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.ObjectDescriptor>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.SchemaVersion = SchemaVersion;
            writer.GameName = GameName;
            writer.SessionStartUnixUs = SessionStartUnixUs;
            Environment?.serialize(writer.Environment);
            writer.Objects.Init(Objects, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ushort SchemaVersion
        {
            get;
            set;
        }

        public string GameName
        {
            get;
            set;
        }

        public long SessionStartUnixUs
        {
            get;
            set;
        }

        public CapnpGen.Environment Environment
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.ObjectDescriptor> Objects
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public ushort SchemaVersion => ctx.ReadDataUShort(0UL, (ushort)0);
            public string GameName => ctx.ReadText(0, null);
            public long SessionStartUnixUs => ctx.ReadDataLong(64UL, 0L);
            public CapnpGen.Environment.READER Environment => ctx.ReadStruct(1, CapnpGen.Environment.READER.create);
            public IReadOnlyList<CapnpGen.ObjectDescriptor.READER> Objects => ctx.ReadList(2).Cast(CapnpGen.ObjectDescriptor.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 3);
            }

            public ushort SchemaVersion
            {
                get => this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, value, (ushort)0);
            }

            public string GameName
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public long SessionStartUnixUs
            {
                get => this.ReadDataLong(64UL, 0L);
                set => this.WriteData(64UL, value, 0L);
            }

            public CapnpGen.Environment.WRITER Environment
            {
                get => BuildPointer<CapnpGen.Environment.WRITER>(1);
                set => Link(1, value);
            }

            public ListOfStructsSerializer<CapnpGen.ObjectDescriptor.WRITER> Objects
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.ObjectDescriptor.WRITER>>(2);
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb100c0db612e5bccUL)]
    public class ObjectDescriptor : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb100c0db612e5bccUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Name = reader.Name;
            Type = reader.Type;
            Location = reader.Location;
            TypeLabel = reader.TypeLabel;
            Primary = reader.Primary;
            ReferencePoint = CapnpSerializable.Create<CapnpGen.NamedPoint>(reader.ReferencePoint);
            NamedPoints = reader.NamedPoints?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.NamedPoint>(_));
            DrivePoints = reader.DrivePoints?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.DrivePointDescriptor>(_));
            Channels = reader.Channels?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.ChannelDescriptor>(_));
            Values = reader.Values?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.ChannelValue>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Name = Name;
            writer.Type = Type;
            writer.Location = Location;
            writer.TypeLabel = TypeLabel;
            writer.Primary = Primary;
            ReferencePoint?.serialize(writer.ReferencePoint);
            writer.NamedPoints.Init(NamedPoints, (_s1, _v1) => _v1?.serialize(_s1));
            writer.DrivePoints.Init(DrivePoints, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Channels.Init(Channels, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Values.Init(Values, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string Name
        {
            get;
            set;
        }

        public CapnpGen.ObjectType Type
        {
            get;
            set;
        }

        public string Location
        {
            get;
            set;
        }

        public string TypeLabel
        {
            get;
            set;
        }

        public bool Primary
        {
            get;
            set;
        }

        public CapnpGen.NamedPoint ReferencePoint
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.NamedPoint> NamedPoints
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.DrivePointDescriptor> DrivePoints
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.ChannelDescriptor> Channels
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.ChannelValue> Values
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public string Name => ctx.ReadText(0, null);
            public CapnpGen.ObjectType Type => (CapnpGen.ObjectType)ctx.ReadDataUShort(0UL, (ushort)0);
            public string Location => ctx.ReadText(1, null);
            public string TypeLabel => ctx.ReadText(2, null);
            public bool Primary => ctx.ReadDataBool(16UL, false);
            public CapnpGen.NamedPoint.READER ReferencePoint => ctx.ReadStruct(3, CapnpGen.NamedPoint.READER.create);
            public IReadOnlyList<CapnpGen.NamedPoint.READER> NamedPoints => ctx.ReadList(4).Cast(CapnpGen.NamedPoint.READER.create);
            public IReadOnlyList<CapnpGen.DrivePointDescriptor.READER> DrivePoints => ctx.ReadList(5).Cast(CapnpGen.DrivePointDescriptor.READER.create);
            public IReadOnlyList<CapnpGen.ChannelDescriptor.READER> Channels => ctx.ReadList(6).Cast(CapnpGen.ChannelDescriptor.READER.create);
            public IReadOnlyList<CapnpGen.ChannelValue.READER> Values => ctx.ReadList(7).Cast(CapnpGen.ChannelValue.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 8);
            }

            public string Name
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public CapnpGen.ObjectType Type
            {
                get => (CapnpGen.ObjectType)this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, (ushort)value, (ushort)0);
            }

            public string Location
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public string TypeLabel
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }

            public bool Primary
            {
                get => this.ReadDataBool(16UL, false);
                set => this.WriteData(16UL, value, false);
            }

            public CapnpGen.NamedPoint.WRITER ReferencePoint
            {
                get => BuildPointer<CapnpGen.NamedPoint.WRITER>(3);
                set => Link(3, value);
            }

            public ListOfStructsSerializer<CapnpGen.NamedPoint.WRITER> NamedPoints
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.NamedPoint.WRITER>>(4);
                set => Link(4, value);
            }

            public ListOfStructsSerializer<CapnpGen.DrivePointDescriptor.WRITER> DrivePoints
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.DrivePointDescriptor.WRITER>>(5);
                set => Link(5, value);
            }

            public ListOfStructsSerializer<CapnpGen.ChannelDescriptor.WRITER> Channels
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.ChannelDescriptor.WRITER>>(6);
                set => Link(6, value);
            }

            public ListOfStructsSerializer<CapnpGen.ChannelValue.WRITER> Values
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.ChannelValue.WRITER>>(7);
                set => Link(7, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe47d16add16e230aUL)]
    public class NamedPoint : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe47d16add16e230aUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Name = reader.Name;
            Position = CapnpSerializable.Create<CapnpGen.Vector3>(reader.Position);
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Name = Name;
            Position?.serialize(writer.Position);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string Name
        {
            get;
            set;
        }

        public CapnpGen.Vector3 Position
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public string Name => ctx.ReadText(0, null);
            public CapnpGen.Vector3.READER Position => ctx.ReadStruct(1, CapnpGen.Vector3.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 2);
            }

            public string Name
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public CapnpGen.Vector3.WRITER Position
            {
                get => BuildPointer<CapnpGen.Vector3.WRITER>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xbb70de632ec7f0a1UL)]
    public class ChannelDescriptor : ICapnpSerializable
    {
        public const UInt64 typeId = 0xbb70de632ec7f0a1UL;
        public enum WHICH : ushort
        {
            NoRange = 0,
            Range = 1,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.NoRange:
                    which = reader.which;
                    break;
                case WHICH.Range:
                    Range = CapnpSerializable.Create<CapnpGen.Range>(reader.Range);
                    break;
            }

            Id = reader.Id;
            Name = reader.Name;
            Unit = reader.Unit;
            Description = reader.Description;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.NoRange:
                        break;
                    case WHICH.Range:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.NoRange:
                    break;
                case WHICH.Range:
                    Range?.serialize(writer.Range);
                    break;
            }

            writer.Id = Id;
            writer.Name = Name;
            writer.Unit = Unit;
            writer.Description = Description;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ushort Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Unit
        {
            get;
            set;
        }

        public CapnpGen.Range Range
        {
            get => _which == WHICH.Range ? (CapnpGen.Range)_content : null;
            set
            {
                _which = WHICH.Range;
                _content = value;
            }
        }

        public string Description
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(16U, (ushort)0);
            public ushort Id => ctx.ReadDataUShort(0UL, (ushort)0);
            public string Name => ctx.ReadText(0, null);
            public string Unit => ctx.ReadText(1, null);
            public CapnpGen.Range.READER Range => which == WHICH.Range ? ctx.ReadStruct(2, CapnpGen.Range.READER.create) : default;
            public string Description => ctx.ReadText(3, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 4);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(16U, (ushort)0);
                set => this.WriteData(16U, (ushort)value, (ushort)0);
            }

            public ushort Id
            {
                get => this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, value, (ushort)0);
            }

            public string Name
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public string Unit
            {
                get => this.ReadText(1, null);
                set => this.WriteText(1, value, null);
            }

            public CapnpGen.Range.WRITER Range
            {
                get => which == WHICH.Range ? BuildPointer<CapnpGen.Range.WRITER>(2) : default;
                set => Link(2, value);
            }

            public string Description
            {
                get => this.ReadText(3, null);
                set => this.WriteText(3, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd6f00ba2dc5e1c4cUL)]
    public class SuspensionSpec : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd6f00ba2dc5e1c4cUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Travel = CapnpSerializable.Create<CapnpGen.Range>(reader.Travel);
            Stiffness = reader.Stiffness;
            Damping = reader.Damping;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            Travel?.serialize(writer.Travel);
            writer.Stiffness = Stiffness;
            writer.Damping = Damping;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.Range Travel
        {
            get;
            set;
        }

        public float Stiffness
        {
            get;
            set;
        }

        public float Damping
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public CapnpGen.Range.READER Travel => ctx.ReadStruct(0, CapnpGen.Range.READER.create);
            public float Stiffness => ctx.ReadDataFloat(0UL, 0F);
            public float Damping => ctx.ReadDataFloat(32UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public CapnpGen.Range.WRITER Travel
            {
                get => BuildPointer<CapnpGen.Range.WRITER>(0);
                set => Link(0, value);
            }

            public float Stiffness
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float Damping
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x83326cebccc07c0dUL)]
    public class DrivePointDescriptor : ICapnpSerializable
    {
        public const UInt64 typeId = 0x83326cebccc07c0dUL;
        public enum WHICH : ushort
        {
            Wheel = 0,
            Propeller = 1,
            Jet = 2,
            Sail = 3,
            Leg = 4,
            Wing = 5,
            Generic = 6,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.Wheel:
                    Wheel = CapnpSerializable.Create<CapnpGen.WheelSpec>(reader.Wheel);
                    break;
                case WHICH.Propeller:
                    Propeller = CapnpSerializable.Create<CapnpGen.PropellerSpec>(reader.Propeller);
                    break;
                case WHICH.Jet:
                    which = reader.which;
                    break;
                case WHICH.Sail:
                    which = reader.which;
                    break;
                case WHICH.Leg:
                    which = reader.which;
                    break;
                case WHICH.Wing:
                    Wing = CapnpSerializable.Create<CapnpGen.WingSpec>(reader.Wing);
                    break;
                case WHICH.Generic:
                    Generic = CapnpSerializable.Create<CapnpGen.GenericSpec>(reader.Generic);
                    break;
            }

            Id = reader.Id;
            Name = reader.Name;
            Offset = CapnpSerializable.Create<CapnpGen.Vector3>(reader.Offset);
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.Wheel:
                        _content = null;
                        break;
                    case WHICH.Propeller:
                        _content = null;
                        break;
                    case WHICH.Jet:
                        break;
                    case WHICH.Sail:
                        break;
                    case WHICH.Leg:
                        break;
                    case WHICH.Wing:
                        _content = null;
                        break;
                    case WHICH.Generic:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.Wheel:
                    Wheel?.serialize(writer.Wheel);
                    break;
                case WHICH.Propeller:
                    Propeller?.serialize(writer.Propeller);
                    break;
                case WHICH.Jet:
                    break;
                case WHICH.Sail:
                    break;
                case WHICH.Leg:
                    break;
                case WHICH.Wing:
                    Wing?.serialize(writer.Wing);
                    break;
                case WHICH.Generic:
                    Generic?.serialize(writer.Generic);
                    break;
            }

            writer.Id = Id;
            writer.Name = Name;
            Offset?.serialize(writer.Offset);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ushort Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public CapnpGen.Vector3 Offset
        {
            get;
            set;
        }

        public CapnpGen.WheelSpec Wheel
        {
            get => _which == WHICH.Wheel ? (CapnpGen.WheelSpec)_content : null;
            set
            {
                _which = WHICH.Wheel;
                _content = value;
            }
        }

        public CapnpGen.PropellerSpec Propeller
        {
            get => _which == WHICH.Propeller ? (CapnpGen.PropellerSpec)_content : null;
            set
            {
                _which = WHICH.Propeller;
                _content = value;
            }
        }

        public CapnpGen.WingSpec Wing
        {
            get => _which == WHICH.Wing ? (CapnpGen.WingSpec)_content : null;
            set
            {
                _which = WHICH.Wing;
                _content = value;
            }
        }

        public CapnpGen.GenericSpec Generic
        {
            get => _which == WHICH.Generic ? (CapnpGen.GenericSpec)_content : null;
            set
            {
                _which = WHICH.Generic;
                _content = value;
            }
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(16U, (ushort)0);
            public ushort Id => ctx.ReadDataUShort(0UL, (ushort)0);
            public string Name => ctx.ReadText(0, null);
            public CapnpGen.Vector3.READER Offset => ctx.ReadStruct(1, CapnpGen.Vector3.READER.create);
            public CapnpGen.WheelSpec.READER Wheel => which == WHICH.Wheel ? ctx.ReadStruct(2, CapnpGen.WheelSpec.READER.create) : default;
            public CapnpGen.PropellerSpec.READER Propeller => which == WHICH.Propeller ? ctx.ReadStruct(2, CapnpGen.PropellerSpec.READER.create) : default;
            public CapnpGen.WingSpec.READER Wing => which == WHICH.Wing ? ctx.ReadStruct(2, CapnpGen.WingSpec.READER.create) : default;
            public CapnpGen.GenericSpec.READER Generic => which == WHICH.Generic ? ctx.ReadStruct(2, CapnpGen.GenericSpec.READER.create) : default;
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 3);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(16U, (ushort)0);
                set => this.WriteData(16U, (ushort)value, (ushort)0);
            }

            public ushort Id
            {
                get => this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, value, (ushort)0);
            }

            public string Name
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public CapnpGen.Vector3.WRITER Offset
            {
                get => BuildPointer<CapnpGen.Vector3.WRITER>(1);
                set => Link(1, value);
            }

            public CapnpGen.WheelSpec.WRITER Wheel
            {
                get => which == WHICH.Wheel ? BuildPointer<CapnpGen.WheelSpec.WRITER>(2) : default;
                set => Link(2, value);
            }

            public CapnpGen.PropellerSpec.WRITER Propeller
            {
                get => which == WHICH.Propeller ? BuildPointer<CapnpGen.PropellerSpec.WRITER>(2) : default;
                set => Link(2, value);
            }

            public CapnpGen.WingSpec.WRITER Wing
            {
                get => which == WHICH.Wing ? BuildPointer<CapnpGen.WingSpec.WRITER>(2) : default;
                set => Link(2, value);
            }

            public CapnpGen.GenericSpec.WRITER Generic
            {
                get => which == WHICH.Generic ? BuildPointer<CapnpGen.GenericSpec.WRITER>(2) : default;
                set => Link(2, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xf2b5d3b40d2f6b06UL)]
    public class WingSpec : ICapnpSerializable
    {
        public const UInt64 typeId = 0xf2b5d3b40d2f6b06UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            FlapRange = CapnpSerializable.Create<CapnpGen.Range>(reader.FlapRange);
            Span = reader.Span;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            FlapRange?.serialize(writer.FlapRange);
            writer.Span = Span;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.Range FlapRange
        {
            get;
            set;
        }

        public float Span
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public CapnpGen.Range.READER FlapRange => ctx.ReadStruct(0, CapnpGen.Range.READER.create);
            public float Span => ctx.ReadDataFloat(0UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public CapnpGen.Range.WRITER FlapRange
            {
                get => BuildPointer<CapnpGen.Range.WRITER>(0);
                set => Link(0, value);
            }

            public float Span
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x8fb350ea71bf9cbeUL)]
    public class WheelSpec : ICapnpSerializable
    {
        public const UInt64 typeId = 0x8fb350ea71bf9cbeUL;
        public enum WHICH : ushort
        {
            NoSuspension = 0,
            Suspension = 1,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.NoSuspension:
                    which = reader.which;
                    break;
                case WHICH.Suspension:
                    Suspension = CapnpSerializable.Create<CapnpGen.SuspensionSpec>(reader.Suspension);
                    break;
            }

            Radius = reader.Radius;
            Driven = reader.Driven;
            Steered = reader.Steered;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.NoSuspension:
                        break;
                    case WHICH.Suspension:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.NoSuspension:
                    break;
                case WHICH.Suspension:
                    Suspension?.serialize(writer.Suspension);
                    break;
            }

            writer.Radius = Radius;
            writer.Driven = Driven;
            writer.Steered = Steered;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float Radius
        {
            get;
            set;
        }

        public bool Driven
        {
            get;
            set;
        }

        public bool Steered
        {
            get;
            set;
        }

        public CapnpGen.SuspensionSpec Suspension
        {
            get => _which == WHICH.Suspension ? (CapnpGen.SuspensionSpec)_content : null;
            set
            {
                _which = WHICH.Suspension;
                _content = value;
            }
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(48U, (ushort)0);
            public float Radius => ctx.ReadDataFloat(0UL, 0F);
            public bool Driven => ctx.ReadDataBool(32UL, false);
            public bool Steered => ctx.ReadDataBool(33UL, false);
            public CapnpGen.SuspensionSpec.READER Suspension => which == WHICH.Suspension ? ctx.ReadStruct(0, CapnpGen.SuspensionSpec.READER.create) : default;
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(48U, (ushort)0);
                set => this.WriteData(48U, (ushort)value, (ushort)0);
            }

            public float Radius
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public bool Driven
            {
                get => this.ReadDataBool(32UL, false);
                set => this.WriteData(32UL, value, false);
            }

            public bool Steered
            {
                get => this.ReadDataBool(33UL, false);
                set => this.WriteData(33UL, value, false);
            }

            public CapnpGen.SuspensionSpec.WRITER Suspension
            {
                get => which == WHICH.Suspension ? BuildPointer<CapnpGen.SuspensionSpec.WRITER>(0) : default;
                set => Link(0, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe92a717ed0e2281fUL)]
    public class PropellerSpec : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe92a717ed0e2281fUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Diameter = reader.Diameter;
            Blades = reader.Blades;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Diameter = Diameter;
            writer.Blades = Blades;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float Diameter
        {
            get;
            set;
        }

        public ushort Blades
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public float Diameter => ctx.ReadDataFloat(0UL, 0F);
            public ushort Blades => ctx.ReadDataUShort(32UL, (ushort)0);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 0);
            }

            public float Diameter
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public ushort Blades
            {
                get => this.ReadDataUShort(32UL, (ushort)0);
                set => this.WriteData(32UL, value, (ushort)0);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc43d09cb526e852dUL)]
    public class GenericSpec : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc43d09cb526e852dUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            RatedForce = reader.RatedForce;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.RatedForce = RatedForce;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float RatedForce
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public float RatedForce => ctx.ReadDataFloat(0UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 0);
            }

            public float RatedForce
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xae9c7165b2164fe6UL)]
    public class MotionFrame : ICapnpSerializable
    {
        public const UInt64 typeId = 0xae9c7165b2164fe6UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Timestamp = reader.Timestamp;
            Objects = reader.Objects?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.MotionObject>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Timestamp = Timestamp;
            writer.Objects.Init(Objects, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public double Timestamp
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.MotionObject> Objects
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public double Timestamp => ctx.ReadDataDouble(0UL, 0);
            public IReadOnlyList<CapnpGen.MotionObject.READER> Objects => ctx.ReadList(0).Cast(CapnpGen.MotionObject.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public double Timestamp
            {
                get => this.ReadDataDouble(0UL, 0);
                set => this.WriteData(0UL, value, 0);
            }

            public ListOfStructsSerializer<CapnpGen.MotionObject.WRITER> Objects
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.MotionObject.WRITER>>(0);
                set => Link(0, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x92b5a7ca58a6082fUL)]
    public class MotionObject : ICapnpSerializable
    {
        public const UInt64 typeId = 0x92b5a7ca58a6082fUL;
        public enum WHICH : ushort
        {
            NoDynamics = 0,
            Kinematics = 1,
            BodyDynamics = 2,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.NoDynamics:
                    which = reader.which;
                    break;
                case WHICH.Kinematics:
                    Kinematics = CapnpSerializable.Create<CapnpGen.Kinematics>(reader.Kinematics);
                    break;
                case WHICH.BodyDynamics:
                    BodyDynamics = CapnpSerializable.Create<CapnpGen.BodyDynamics>(reader.BodyDynamics);
                    break;
            }

            Name = reader.Name;
            Position = CapnpSerializable.Create<CapnpGen.Vector3>(reader.Position);
            Orientation = CapnpSerializable.Create<CapnpGen.Quaternion>(reader.Orientation);
            DrivePoints = reader.DrivePoints?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.DrivePoint>(_));
            Channels = reader.Channels?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.ChannelValue>(_));
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.NoDynamics:
                        break;
                    case WHICH.Kinematics:
                        _content = null;
                        break;
                    case WHICH.BodyDynamics:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.NoDynamics:
                    break;
                case WHICH.Kinematics:
                    Kinematics?.serialize(writer.Kinematics);
                    break;
                case WHICH.BodyDynamics:
                    BodyDynamics?.serialize(writer.BodyDynamics);
                    break;
            }

            writer.Name = Name;
            Position?.serialize(writer.Position);
            Orientation?.serialize(writer.Orientation);
            writer.DrivePoints.Init(DrivePoints, (_s1, _v1) => _v1?.serialize(_s1));
            writer.Channels.Init(Channels, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public string Name
        {
            get;
            set;
        }

        public CapnpGen.Vector3 Position
        {
            get;
            set;
        }

        public CapnpGen.Quaternion Orientation
        {
            get;
            set;
        }

        public CapnpGen.Kinematics Kinematics
        {
            get => _which == WHICH.Kinematics ? (CapnpGen.Kinematics)_content : null;
            set
            {
                _which = WHICH.Kinematics;
                _content = value;
            }
        }

        public CapnpGen.BodyDynamics BodyDynamics
        {
            get => _which == WHICH.BodyDynamics ? (CapnpGen.BodyDynamics)_content : null;
            set
            {
                _which = WHICH.BodyDynamics;
                _content = value;
            }
        }

        public IReadOnlyList<CapnpGen.DrivePoint> DrivePoints
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.ChannelValue> Channels
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(0U, (ushort)0);
            public string Name => ctx.ReadText(0, null);
            public CapnpGen.Vector3.READER Position => ctx.ReadStruct(1, CapnpGen.Vector3.READER.create);
            public CapnpGen.Quaternion.READER Orientation => ctx.ReadStruct(2, CapnpGen.Quaternion.READER.create);
            public CapnpGen.Kinematics.READER Kinematics => which == WHICH.Kinematics ? ctx.ReadStruct(3, CapnpGen.Kinematics.READER.create) : default;
            public CapnpGen.BodyDynamics.READER BodyDynamics => which == WHICH.BodyDynamics ? ctx.ReadStruct(3, CapnpGen.BodyDynamics.READER.create) : default;
            public IReadOnlyList<CapnpGen.DrivePoint.READER> DrivePoints => ctx.ReadList(4).Cast(CapnpGen.DrivePoint.READER.create);
            public IReadOnlyList<CapnpGen.ChannelValue.READER> Channels => ctx.ReadList(5).Cast(CapnpGen.ChannelValue.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 6);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(0U, (ushort)0);
                set => this.WriteData(0U, (ushort)value, (ushort)0);
            }

            public string Name
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public CapnpGen.Vector3.WRITER Position
            {
                get => BuildPointer<CapnpGen.Vector3.WRITER>(1);
                set => Link(1, value);
            }

            public CapnpGen.Quaternion.WRITER Orientation
            {
                get => BuildPointer<CapnpGen.Quaternion.WRITER>(2);
                set => Link(2, value);
            }

            public CapnpGen.Kinematics.WRITER Kinematics
            {
                get => which == WHICH.Kinematics ? BuildPointer<CapnpGen.Kinematics.WRITER>(3) : default;
                set => Link(3, value);
            }

            public CapnpGen.BodyDynamics.WRITER BodyDynamics
            {
                get => which == WHICH.BodyDynamics ? BuildPointer<CapnpGen.BodyDynamics.WRITER>(3) : default;
                set => Link(3, value);
            }

            public ListOfStructsSerializer<CapnpGen.DrivePoint.WRITER> DrivePoints
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.DrivePoint.WRITER>>(4);
                set => Link(4, value);
            }

            public ListOfStructsSerializer<CapnpGen.ChannelValue.WRITER> Channels
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.ChannelValue.WRITER>>(5);
                set => Link(5, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xef3d2dfde94c8e55UL)]
    public class Kinematics : ICapnpSerializable
    {
        public const UInt64 typeId = 0xef3d2dfde94c8e55UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Velocity = CapnpSerializable.Create<CapnpGen.Vector3>(reader.Velocity);
            Acceleration = CapnpSerializable.Create<CapnpGen.Vector3>(reader.Acceleration);
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            Velocity?.serialize(writer.Velocity);
            Acceleration?.serialize(writer.Acceleration);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.Vector3 Velocity
        {
            get;
            set;
        }

        public CapnpGen.Vector3 Acceleration
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public CapnpGen.Vector3.READER Velocity => ctx.ReadStruct(0, CapnpGen.Vector3.READER.create);
            public CapnpGen.Vector3.READER Acceleration => ctx.ReadStruct(1, CapnpGen.Vector3.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 2);
            }

            public CapnpGen.Vector3.WRITER Velocity
            {
                get => BuildPointer<CapnpGen.Vector3.WRITER>(0);
                set => Link(0, value);
            }

            public CapnpGen.Vector3.WRITER Acceleration
            {
                get => BuildPointer<CapnpGen.Vector3.WRITER>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xc8753ef8c2c02116UL)]
    public class BodyDynamics : ICapnpSerializable
    {
        public const UInt64 typeId = 0xc8753ef8c2c02116UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            SpecificForce = CapnpSerializable.Create<CapnpGen.Vector3>(reader.SpecificForce);
            AngularVelocity = CapnpSerializable.Create<CapnpGen.Vector3>(reader.AngularVelocity);
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            SpecificForce?.serialize(writer.SpecificForce);
            AngularVelocity?.serialize(writer.AngularVelocity);
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public CapnpGen.Vector3 SpecificForce
        {
            get;
            set;
        }

        public CapnpGen.Vector3 AngularVelocity
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public CapnpGen.Vector3.READER SpecificForce => ctx.ReadStruct(0, CapnpGen.Vector3.READER.create);
            public CapnpGen.Vector3.READER AngularVelocity => ctx.ReadStruct(1, CapnpGen.Vector3.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(0, 2);
            }

            public CapnpGen.Vector3.WRITER SpecificForce
            {
                get => BuildPointer<CapnpGen.Vector3.WRITER>(0);
                set => Link(0, value);
            }

            public CapnpGen.Vector3.WRITER AngularVelocity
            {
                get => BuildPointer<CapnpGen.Vector3.WRITER>(1);
                set => Link(1, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xeb2d45a66cc7d9e7UL)]
    public class DrivePoint : ICapnpSerializable
    {
        public const UInt64 typeId = 0xeb2d45a66cc7d9e7UL;
        public enum WHICH : ushort
        {
            Wheel = 0,
            Propeller = 1,
            Jet = 2,
            Sail = 3,
            Leg = 4,
            Wing = 5,
            Generic = 6,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.Wheel:
                    Wheel = CapnpSerializable.Create<CapnpGen.WheelState>(reader.Wheel);
                    break;
                case WHICH.Propeller:
                    Propeller = CapnpSerializable.Create<CapnpGen.PropellerState>(reader.Propeller);
                    break;
                case WHICH.Jet:
                    Jet = CapnpSerializable.Create<CapnpGen.JetState>(reader.Jet);
                    break;
                case WHICH.Sail:
                    Sail = CapnpSerializable.Create<CapnpGen.SailState>(reader.Sail);
                    break;
                case WHICH.Leg:
                    Leg = CapnpSerializable.Create<CapnpGen.LegState>(reader.Leg);
                    break;
                case WHICH.Wing:
                    Wing = CapnpSerializable.Create<CapnpGen.WingState>(reader.Wing);
                    break;
                case WHICH.Generic:
                    Generic = CapnpSerializable.Create<CapnpGen.GenericState>(reader.Generic);
                    break;
            }

            Id = reader.Id;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.Wheel:
                        _content = null;
                        break;
                    case WHICH.Propeller:
                        _content = null;
                        break;
                    case WHICH.Jet:
                        _content = null;
                        break;
                    case WHICH.Sail:
                        _content = null;
                        break;
                    case WHICH.Leg:
                        _content = null;
                        break;
                    case WHICH.Wing:
                        _content = null;
                        break;
                    case WHICH.Generic:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.Wheel:
                    Wheel?.serialize(writer.Wheel);
                    break;
                case WHICH.Propeller:
                    Propeller?.serialize(writer.Propeller);
                    break;
                case WHICH.Jet:
                    Jet?.serialize(writer.Jet);
                    break;
                case WHICH.Sail:
                    Sail?.serialize(writer.Sail);
                    break;
                case WHICH.Leg:
                    Leg?.serialize(writer.Leg);
                    break;
                case WHICH.Wing:
                    Wing?.serialize(writer.Wing);
                    break;
                case WHICH.Generic:
                    Generic?.serialize(writer.Generic);
                    break;
            }

            writer.Id = Id;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ushort Id
        {
            get;
            set;
        }

        public CapnpGen.WheelState Wheel
        {
            get => _which == WHICH.Wheel ? (CapnpGen.WheelState)_content : null;
            set
            {
                _which = WHICH.Wheel;
                _content = value;
            }
        }

        public CapnpGen.PropellerState Propeller
        {
            get => _which == WHICH.Propeller ? (CapnpGen.PropellerState)_content : null;
            set
            {
                _which = WHICH.Propeller;
                _content = value;
            }
        }

        public CapnpGen.JetState Jet
        {
            get => _which == WHICH.Jet ? (CapnpGen.JetState)_content : null;
            set
            {
                _which = WHICH.Jet;
                _content = value;
            }
        }

        public CapnpGen.SailState Sail
        {
            get => _which == WHICH.Sail ? (CapnpGen.SailState)_content : null;
            set
            {
                _which = WHICH.Sail;
                _content = value;
            }
        }

        public CapnpGen.LegState Leg
        {
            get => _which == WHICH.Leg ? (CapnpGen.LegState)_content : null;
            set
            {
                _which = WHICH.Leg;
                _content = value;
            }
        }

        public CapnpGen.WingState Wing
        {
            get => _which == WHICH.Wing ? (CapnpGen.WingState)_content : null;
            set
            {
                _which = WHICH.Wing;
                _content = value;
            }
        }

        public CapnpGen.GenericState Generic
        {
            get => _which == WHICH.Generic ? (CapnpGen.GenericState)_content : null;
            set
            {
                _which = WHICH.Generic;
                _content = value;
            }
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(16U, (ushort)0);
            public ushort Id => ctx.ReadDataUShort(0UL, (ushort)0);
            public CapnpGen.WheelState.READER Wheel => which == WHICH.Wheel ? ctx.ReadStruct(0, CapnpGen.WheelState.READER.create) : default;
            public CapnpGen.PropellerState.READER Propeller => which == WHICH.Propeller ? ctx.ReadStruct(0, CapnpGen.PropellerState.READER.create) : default;
            public CapnpGen.JetState.READER Jet => which == WHICH.Jet ? ctx.ReadStruct(0, CapnpGen.JetState.READER.create) : default;
            public CapnpGen.SailState.READER Sail => which == WHICH.Sail ? ctx.ReadStruct(0, CapnpGen.SailState.READER.create) : default;
            public CapnpGen.LegState.READER Leg => which == WHICH.Leg ? ctx.ReadStruct(0, CapnpGen.LegState.READER.create) : default;
            public CapnpGen.WingState.READER Wing => which == WHICH.Wing ? ctx.ReadStruct(0, CapnpGen.WingState.READER.create) : default;
            public CapnpGen.GenericState.READER Generic => which == WHICH.Generic ? ctx.ReadStruct(0, CapnpGen.GenericState.READER.create) : default;
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(16U, (ushort)0);
                set => this.WriteData(16U, (ushort)value, (ushort)0);
            }

            public ushort Id
            {
                get => this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, value, (ushort)0);
            }

            public CapnpGen.WheelState.WRITER Wheel
            {
                get => which == WHICH.Wheel ? BuildPointer<CapnpGen.WheelState.WRITER>(0) : default;
                set => Link(0, value);
            }

            public CapnpGen.PropellerState.WRITER Propeller
            {
                get => which == WHICH.Propeller ? BuildPointer<CapnpGen.PropellerState.WRITER>(0) : default;
                set => Link(0, value);
            }

            public CapnpGen.JetState.WRITER Jet
            {
                get => which == WHICH.Jet ? BuildPointer<CapnpGen.JetState.WRITER>(0) : default;
                set => Link(0, value);
            }

            public CapnpGen.SailState.WRITER Sail
            {
                get => which == WHICH.Sail ? BuildPointer<CapnpGen.SailState.WRITER>(0) : default;
                set => Link(0, value);
            }

            public CapnpGen.LegState.WRITER Leg
            {
                get => which == WHICH.Leg ? BuildPointer<CapnpGen.LegState.WRITER>(0) : default;
                set => Link(0, value);
            }

            public CapnpGen.WingState.WRITER Wing
            {
                get => which == WHICH.Wing ? BuildPointer<CapnpGen.WingState.WRITER>(0) : default;
                set => Link(0, value);
            }

            public CapnpGen.GenericState.WRITER Generic
            {
                get => which == WHICH.Generic ? BuildPointer<CapnpGen.GenericState.WRITER>(0) : default;
                set => Link(0, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xe81c8acddcdbb52eUL)]
    public class WheelState : ICapnpSerializable
    {
        public const UInt64 typeId = 0xe81c8acddcdbb52eUL;
        public enum WHICH : ushort
        {
            Core = 0,
            Extended = 1,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.Core:
                    which = reader.which;
                    break;
                case WHICH.Extended:
                    Extended = CapnpSerializable.Create<CapnpGen.WheelExtended>(reader.Extended);
                    break;
            }

            Rpm = reader.Rpm;
            Torque = reader.Torque;
            BrakePressure = reader.BrakePressure;
            Slip = reader.Slip;
            Compression = reader.Compression;
            Contact = reader.Contact;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.Core:
                        break;
                    case WHICH.Extended:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.Core:
                    break;
                case WHICH.Extended:
                    Extended?.serialize(writer.Extended);
                    break;
            }

            writer.Rpm = Rpm;
            writer.Torque = Torque;
            writer.BrakePressure = BrakePressure;
            writer.Slip = Slip;
            writer.Compression = Compression;
            writer.Contact = Contact;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float Rpm
        {
            get;
            set;
        }

        public float Torque
        {
            get;
            set;
        }

        public float BrakePressure
        {
            get;
            set;
        }

        public float Slip
        {
            get;
            set;
        }

        public float Compression
        {
            get;
            set;
        }

        public CapnpGen.ContactMedium Contact
        {
            get;
            set;
        }

        public CapnpGen.WheelExtended Extended
        {
            get => _which == WHICH.Extended ? (CapnpGen.WheelExtended)_content : null;
            set
            {
                _which = WHICH.Extended;
                _content = value;
            }
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(176U, (ushort)0);
            public float Rpm => ctx.ReadDataFloat(0UL, 0F);
            public float Torque => ctx.ReadDataFloat(32UL, 0F);
            public float BrakePressure => ctx.ReadDataFloat(64UL, 0F);
            public float Slip => ctx.ReadDataFloat(96UL, 0F);
            public float Compression => ctx.ReadDataFloat(128UL, 0F);
            public CapnpGen.ContactMedium Contact => (CapnpGen.ContactMedium)ctx.ReadDataUShort(160UL, (ushort)0);
            public CapnpGen.WheelExtended.READER Extended => which == WHICH.Extended ? ctx.ReadStruct(0, CapnpGen.WheelExtended.READER.create) : default;
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 1);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(176U, (ushort)0);
                set => this.WriteData(176U, (ushort)value, (ushort)0);
            }

            public float Rpm
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float Torque
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float BrakePressure
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public float Slip
            {
                get => this.ReadDataFloat(96UL, 0F);
                set => this.WriteData(96UL, value, 0F);
            }

            public float Compression
            {
                get => this.ReadDataFloat(128UL, 0F);
                set => this.WriteData(128UL, value, 0F);
            }

            public CapnpGen.ContactMedium Contact
            {
                get => (CapnpGen.ContactMedium)this.ReadDataUShort(160UL, (ushort)0);
                set => this.WriteData(160UL, (ushort)value, (ushort)0);
            }

            public CapnpGen.WheelExtended.WRITER Extended
            {
                get => which == WHICH.Extended ? BuildPointer<CapnpGen.WheelExtended.WRITER>(0) : default;
                set => Link(0, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x9ae27747bcccb978UL)]
    public class WheelExtended : ICapnpSerializable
    {
        public const UInt64 typeId = 0x9ae27747bcccb978UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            SteerAngle = reader.SteerAngle;
            LoadForce = reader.LoadForce;
            DamperVelocity = reader.DamperVelocity;
            TyrePressure = reader.TyrePressure;
            TyreTemp = reader.TyreTemp;
            TyreWear = reader.TyreWear;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.SteerAngle = SteerAngle;
            writer.LoadForce = LoadForce;
            writer.DamperVelocity = DamperVelocity;
            writer.TyrePressure = TyrePressure;
            writer.TyreTemp = TyreTemp;
            writer.TyreWear = TyreWear;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float SteerAngle
        {
            get;
            set;
        }

        public float LoadForce
        {
            get;
            set;
        }

        public float DamperVelocity
        {
            get;
            set;
        }

        public float TyrePressure
        {
            get;
            set;
        }

        public float TyreTemp
        {
            get;
            set;
        }

        public float TyreWear
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public float SteerAngle => ctx.ReadDataFloat(0UL, 0F);
            public float LoadForce => ctx.ReadDataFloat(32UL, 0F);
            public float DamperVelocity => ctx.ReadDataFloat(64UL, 0F);
            public float TyrePressure => ctx.ReadDataFloat(96UL, 0F);
            public float TyreTemp => ctx.ReadDataFloat(128UL, 0F);
            public float TyreWear => ctx.ReadDataFloat(160UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(3, 0);
            }

            public float SteerAngle
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float LoadForce
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float DamperVelocity
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public float TyrePressure
            {
                get => this.ReadDataFloat(96UL, 0F);
                set => this.WriteData(96UL, value, 0F);
            }

            public float TyreTemp
            {
                get => this.ReadDataFloat(128UL, 0F);
                set => this.WriteData(128UL, value, 0F);
            }

            public float TyreWear
            {
                get => this.ReadDataFloat(160UL, 0F);
                set => this.WriteData(160UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd5c2f109f2924ec6UL)]
    public class PropellerState : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd5c2f109f2924ec6UL;
        public enum WHICH : ushort
        {
            Core = 0,
            Extended = 1,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.Core:
                    which = reader.which;
                    break;
                case WHICH.Extended:
                    Extended = CapnpSerializable.Create<CapnpGen.PropellerExtended>(reader.Extended);
                    break;
            }

            Rpm = reader.Rpm;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.Core:
                        break;
                    case WHICH.Extended:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.Core:
                    break;
                case WHICH.Extended:
                    Extended?.serialize(writer.Extended);
                    break;
            }

            writer.Rpm = Rpm;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float Rpm
        {
            get;
            set;
        }

        public CapnpGen.PropellerExtended Extended
        {
            get => _which == WHICH.Extended ? (CapnpGen.PropellerExtended)_content : null;
            set
            {
                _which = WHICH.Extended;
                _content = value;
            }
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(32U, (ushort)0);
            public float Rpm => ctx.ReadDataFloat(0UL, 0F);
            public CapnpGen.PropellerExtended.READER Extended => which == WHICH.Extended ? ctx.ReadStruct(0, CapnpGen.PropellerExtended.READER.create) : default;
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(32U, (ushort)0);
                set => this.WriteData(32U, (ushort)value, (ushort)0);
            }

            public float Rpm
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public CapnpGen.PropellerExtended.WRITER Extended
            {
                get => which == WHICH.Extended ? BuildPointer<CapnpGen.PropellerExtended.WRITER>(0) : default;
                set => Link(0, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb080bb3601b2b7eaUL)]
    public class PropellerExtended : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb080bb3601b2b7eaUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Pitch = reader.Pitch;
            Thrust = reader.Thrust;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Pitch = Pitch;
            writer.Thrust = Thrust;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float Pitch
        {
            get;
            set;
        }

        public float Thrust
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public float Pitch => ctx.ReadDataFloat(0UL, 0F);
            public float Thrust => ctx.ReadDataFloat(32UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 0);
            }

            public float Pitch
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float Thrust
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xeae12e186bc218d5UL)]
    public class JetState : ICapnpSerializable
    {
        public const UInt64 typeId = 0xeae12e186bc218d5UL;
        public enum WHICH : ushort
        {
            NoThrust = 0,
            Thrust = 1,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.NoThrust:
                    which = reader.which;
                    break;
                case WHICH.Thrust:
                    Thrust = reader.Thrust;
                    break;
            }

            Throttle = reader.Throttle;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.NoThrust:
                        break;
                    case WHICH.Thrust:
                        _content = 0F;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.NoThrust:
                    break;
                case WHICH.Thrust:
                    writer.Thrust = Thrust.Value;
                    break;
            }

            writer.Throttle = Throttle;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float Throttle
        {
            get;
            set;
        }

        public float? Thrust
        {
            get => _which == WHICH.Thrust ? (float? )_content : null;
            set
            {
                _which = WHICH.Thrust;
                _content = value;
            }
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(32U, (ushort)0);
            public float Throttle => ctx.ReadDataFloat(0UL, 0F);
            public float Thrust => which == WHICH.Thrust ? ctx.ReadDataFloat(64UL, 0F) : default;
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 0);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(32U, (ushort)0);
                set => this.WriteData(32U, (ushort)value, (ushort)0);
            }

            public float Throttle
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float Thrust
            {
                get => which == WHICH.Thrust ? this.ReadDataFloat(64UL, 0F) : default;
                set => this.WriteData(64UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xd345fc62a96930adUL)]
    public class SailState : ICapnpSerializable
    {
        public const UInt64 typeId = 0xd345fc62a96930adUL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Sheet = reader.Sheet;
            Angle = reader.Angle;
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Sheet = Sheet;
            writer.Angle = Angle;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float Sheet
        {
            get;
            set;
        }

        public float Angle
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public float Sheet => ctx.ReadDataFloat(0UL, 0F);
            public float Angle => ctx.ReadDataFloat(32UL, 0F);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 0);
            }

            public float Sheet
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public float Angle
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xb952c56e4674ce6eUL)]
    public class LegState : ICapnpSerializable
    {
        public const UInt64 typeId = 0xb952c56e4674ce6eUL;
        public enum WHICH : ushort
        {
            NoForce = 0,
            Force = 1,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.NoForce:
                    which = reader.which;
                    break;
                case WHICH.Force:
                    Force = CapnpSerializable.Create<CapnpGen.Vector3>(reader.Force);
                    break;
            }

            Contact = reader.Contact;
            Phase = reader.Phase;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.NoForce:
                        break;
                    case WHICH.Force:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.NoForce:
                    break;
                case WHICH.Force:
                    Force?.serialize(writer.Force);
                    break;
            }

            writer.Contact = Contact;
            writer.Phase = Phase;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public bool Contact
        {
            get;
            set;
        }

        public float Phase
        {
            get;
            set;
        }

        public CapnpGen.Vector3 Force
        {
            get => _which == WHICH.Force ? (CapnpGen.Vector3)_content : null;
            set
            {
                _which = WHICH.Force;
                _content = value;
            }
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(16U, (ushort)0);
            public bool Contact => ctx.ReadDataBool(0UL, false);
            public float Phase => ctx.ReadDataFloat(32UL, 0F);
            public CapnpGen.Vector3.READER Force => which == WHICH.Force ? ctx.ReadStruct(0, CapnpGen.Vector3.READER.create) : default;
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(16U, (ushort)0);
                set => this.WriteData(16U, (ushort)value, (ushort)0);
            }

            public bool Contact
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public float Phase
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public CapnpGen.Vector3.WRITER Force
            {
                get => which == WHICH.Force ? BuildPointer<CapnpGen.Vector3.WRITER>(0) : default;
                set => Link(0, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0x97200443d484c5fdUL)]
    public class GenericState : ICapnpSerializable
    {
        public const UInt64 typeId = 0x97200443d484c5fdUL;
        public enum WHICH : ushort
        {
            NoForce = 0,
            Force = 1,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.NoForce:
                    which = reader.which;
                    break;
                case WHICH.Force:
                    Force = CapnpSerializable.Create<CapnpGen.Vector3>(reader.Force);
                    break;
            }

            Engaged = reader.Engaged;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.NoForce:
                        break;
                    case WHICH.Force:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.NoForce:
                    break;
                case WHICH.Force:
                    Force?.serialize(writer.Force);
                    break;
            }

            writer.Engaged = Engaged;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public bool Engaged
        {
            get;
            set;
        }

        public CapnpGen.Vector3 Force
        {
            get => _which == WHICH.Force ? (CapnpGen.Vector3)_content : null;
            set
            {
                _which = WHICH.Force;
                _content = value;
            }
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(16U, (ushort)0);
            public bool Engaged => ctx.ReadDataBool(0UL, false);
            public CapnpGen.Vector3.READER Force => which == WHICH.Force ? ctx.ReadStruct(0, CapnpGen.Vector3.READER.create) : default;
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(16U, (ushort)0);
                set => this.WriteData(16U, (ushort)value, (ushort)0);
            }

            public bool Engaged
            {
                get => this.ReadDataBool(0UL, false);
                set => this.WriteData(0UL, value, false);
            }

            public CapnpGen.Vector3.WRITER Force
            {
                get => which == WHICH.Force ? BuildPointer<CapnpGen.Vector3.WRITER>(0) : default;
                set => Link(0, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xea53b1cf1fb46f25UL)]
    public class WingState : ICapnpSerializable
    {
        public const UInt64 typeId = 0xea53b1cf1fb46f25UL;
        public enum WHICH : ushort
        {
            NoForce = 0,
            Force = 1,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.NoForce:
                    which = reader.which;
                    break;
                case WHICH.Force:
                    Force = CapnpSerializable.Create<CapnpGen.Vector3>(reader.Force);
                    break;
            }

            FlapAngle = reader.FlapAngle;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.NoForce:
                        break;
                    case WHICH.Force:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.NoForce:
                    break;
                case WHICH.Force:
                    Force?.serialize(writer.Force);
                    break;
            }

            writer.FlapAngle = FlapAngle;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public float FlapAngle
        {
            get;
            set;
        }

        public CapnpGen.Vector3 Force
        {
            get => _which == WHICH.Force ? (CapnpGen.Vector3)_content : null;
            set
            {
                _which = WHICH.Force;
                _content = value;
            }
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(32U, (ushort)0);
            public float FlapAngle => ctx.ReadDataFloat(0UL, 0F);
            public CapnpGen.Vector3.READER Force => which == WHICH.Force ? ctx.ReadStruct(0, CapnpGen.Vector3.READER.create) : default;
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(32U, (ushort)0);
                set => this.WriteData(32U, (ushort)value, (ushort)0);
            }

            public float FlapAngle
            {
                get => this.ReadDataFloat(0UL, 0F);
                set => this.WriteData(0UL, value, 0F);
            }

            public CapnpGen.Vector3.WRITER Force
            {
                get => which == WHICH.Force ? BuildPointer<CapnpGen.Vector3.WRITER>(0) : default;
                set => Link(0, value);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xdf37d59f71ec23adUL)]
    public class ChannelValue : ICapnpSerializable
    {
        public const UInt64 typeId = 0xdf37d59f71ec23adUL;
        public enum WHICH : ushort
        {
            Number = 0,
            Boolean = 1,
            Text = 2,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.Number:
                    Number = reader.Number;
                    break;
                case WHICH.Boolean:
                    Boolean = reader.Boolean;
                    break;
                case WHICH.Text:
                    Text = reader.Text;
                    break;
            }

            Id = reader.Id;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.Number:
                        _content = 0F;
                        break;
                    case WHICH.Boolean:
                        _content = false;
                        break;
                    case WHICH.Text:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.Number:
                    writer.Number = Number.Value;
                    break;
                case WHICH.Boolean:
                    writer.Boolean = Boolean.Value;
                    break;
                case WHICH.Text:
                    writer.Text = Text;
                    break;
            }

            writer.Id = Id;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public ushort Id
        {
            get;
            set;
        }

        public float? Number
        {
            get => _which == WHICH.Number ? (float? )_content : null;
            set
            {
                _which = WHICH.Number;
                _content = value;
            }
        }

        public bool? Boolean
        {
            get => _which == WHICH.Boolean ? (bool? )_content : null;
            set
            {
                _which = WHICH.Boolean;
                _content = value;
            }
        }

        public string Text
        {
            get => _which == WHICH.Text ? (string)_content : null;
            set
            {
                _which = WHICH.Text;
                _content = value;
            }
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(16U, (ushort)0);
            public ushort Id => ctx.ReadDataUShort(0UL, (ushort)0);
            public float Number => which == WHICH.Number ? ctx.ReadDataFloat(32UL, 0F) : default;
            public bool Boolean => which == WHICH.Boolean ? ctx.ReadDataBool(32UL, false) : default;
            public string Text => which == WHICH.Text ? ctx.ReadText(0, null) : default;
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(16U, (ushort)0);
                set => this.WriteData(16U, (ushort)value, (ushort)0);
            }

            public ushort Id
            {
                get => this.ReadDataUShort(0UL, (ushort)0);
                set => this.WriteData(0UL, value, (ushort)0);
            }

            public float Number
            {
                get => which == WHICH.Number ? this.ReadDataFloat(32UL, 0F) : default;
                set => this.WriteData(32UL, value, 0F);
            }

            public bool Boolean
            {
                get => which == WHICH.Boolean ? this.ReadDataBool(32UL, false) : default;
                set => this.WriteData(32UL, value, false);
            }

            public string Text
            {
                get => which == WHICH.Text ? this.ReadText(0, null) : default;
                set => this.WriteText(0, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xa53e4e94da9f43e9UL)]
    public class Event : ICapnpSerializable
    {
        public const UInt64 typeId = 0xa53e4e94da9f43e9UL;
        public enum WHICH : ushort
        {
            NoDirection = 0,
            Direction = 1,
            undefined = 65535
        }

        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            switch (reader.which)
            {
                case WHICH.NoDirection:
                    which = reader.which;
                    break;
                case WHICH.Direction:
                    Direction = CapnpSerializable.Create<CapnpGen.Vector3>(reader.Direction);
                    break;
            }

            Id = reader.Id;
            Name = reader.Name;
            Intensity = reader.Intensity;
            Duration = reader.Duration;
            Object = reader.Object;
            applyDefaults();
        }

        private WHICH _which = WHICH.undefined;
        private object _content;
        public WHICH which
        {
            get => _which;
            set
            {
                if (value == _which)
                    return;
                _which = value;
                switch (value)
                {
                    case WHICH.NoDirection:
                        break;
                    case WHICH.Direction:
                        _content = null;
                        break;
                }
            }
        }

        public void serialize(WRITER writer)
        {
            writer.which = which;
            switch (which)
            {
                case WHICH.NoDirection:
                    break;
                case WHICH.Direction:
                    Direction?.serialize(writer.Direction);
                    break;
            }

            writer.Id = Id;
            writer.Name = Name;
            writer.Intensity = Intensity;
            writer.Duration = Duration;
            writer.Object = Object;
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public uint Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public float Intensity
        {
            get;
            set;
        }

        public float Duration
        {
            get;
            set;
        }

        public CapnpGen.Vector3 Direction
        {
            get => _which == WHICH.Direction ? (CapnpGen.Vector3)_content : null;
            set
            {
                _which = WHICH.Direction;
                _content = value;
            }
        }

        public string Object
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public WHICH which => (WHICH)ctx.ReadDataUShort(96U, (ushort)0);
            public uint Id => ctx.ReadDataUInt(0UL, 0U);
            public string Name => ctx.ReadText(0, null);
            public float Intensity => ctx.ReadDataFloat(32UL, 0F);
            public float Duration => ctx.ReadDataFloat(64UL, 0F);
            public CapnpGen.Vector3.READER Direction => which == WHICH.Direction ? ctx.ReadStruct(1, CapnpGen.Vector3.READER.create) : default;
            public string Object => ctx.ReadText(2, null);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(2, 3);
            }

            public WHICH which
            {
                get => (WHICH)this.ReadDataUShort(96U, (ushort)0);
                set => this.WriteData(96U, (ushort)value, (ushort)0);
            }

            public uint Id
            {
                get => this.ReadDataUInt(0UL, 0U);
                set => this.WriteData(0UL, value, 0U);
            }

            public string Name
            {
                get => this.ReadText(0, null);
                set => this.WriteText(0, value, null);
            }

            public float Intensity
            {
                get => this.ReadDataFloat(32UL, 0F);
                set => this.WriteData(32UL, value, 0F);
            }

            public float Duration
            {
                get => this.ReadDataFloat(64UL, 0F);
                set => this.WriteData(64UL, value, 0F);
            }

            public CapnpGen.Vector3.WRITER Direction
            {
                get => which == WHICH.Direction ? BuildPointer<CapnpGen.Vector3.WRITER>(1) : default;
                set => Link(1, value);
            }

            public string Object
            {
                get => this.ReadText(2, null);
                set => this.WriteText(2, value, null);
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("capnpc-csharp", "1.3.0.0"), TypeId(0xecfff2f711283428UL)]
    public class EventFrame : ICapnpSerializable
    {
        public const UInt64 typeId = 0xecfff2f711283428UL;
        void ICapnpSerializable.Deserialize(DeserializerState arg_)
        {
            var reader = READER.create(arg_);
            Timestamp = reader.Timestamp;
            Events = reader.Events?.ToReadOnlyList(_ => CapnpSerializable.Create<CapnpGen.Event>(_));
            applyDefaults();
        }

        public void serialize(WRITER writer)
        {
            writer.Timestamp = Timestamp;
            writer.Events.Init(Events, (_s1, _v1) => _v1?.serialize(_s1));
        }

        void ICapnpSerializable.Serialize(SerializerState arg_)
        {
            serialize(arg_.Rewrap<WRITER>());
        }

        public void applyDefaults()
        {
        }

        public double Timestamp
        {
            get;
            set;
        }

        public IReadOnlyList<CapnpGen.Event> Events
        {
            get;
            set;
        }

        public struct READER
        {
            readonly DeserializerState ctx;
            public READER(DeserializerState ctx)
            {
                this.ctx = ctx;
            }

            public static READER create(DeserializerState ctx) => new READER(ctx);
            public static implicit operator DeserializerState(READER reader) => reader.ctx;
            public static implicit operator READER(DeserializerState ctx) => new READER(ctx);
            public double Timestamp => ctx.ReadDataDouble(0UL, 0);
            public IReadOnlyList<CapnpGen.Event.READER> Events => ctx.ReadList(0).Cast(CapnpGen.Event.READER.create);
        }

        public class WRITER : SerializerState
        {
            public WRITER()
            {
                this.SetStruct(1, 1);
            }

            public double Timestamp
            {
                get => this.ReadDataDouble(0UL, 0);
                set => this.WriteData(0UL, value, 0);
            }

            public ListOfStructsSerializer<CapnpGen.Event.WRITER> Events
            {
                get => BuildPointer<ListOfStructsSerializer<CapnpGen.Event.WRITER>>(0);
                set => Link(0, value);
            }
        }
    }
}
#pragma warning restore 8600,8601,8602,8603,8604,8618,8619,8625,8629,8632
