using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ROMTestObjects.RoomObjects.Funny
{
    public class FunnyObject : UpdatableAndDeletable
    {
        public float JustAFloat { get; set; }
        public int JustAnInt { get; set; }

        public float NormFloat { get; set; }
        public float PlusMinusTenFloat { get; set; }
        public int PlusFiveMinusThreeInt { get; set; }
        public int PlusFiveMinusThreeInt2 { get; set; }
        public char CursedChar { get; set; }
        public Vector2[] Polygon { get; set; } =
        {
            new Vector2(500, 500),
            new Vector2(500, 600),
            new Vector2(600, 600),
            new Vector2(600, 500)
        };

        public bool AFlag { get; set; }

        public FunnyEnum AnyEnum { get; set; } = FunnyEnum.Boring;
        public FunnyEnum BoringEnum { get; set;} = FunnyEnum.Boring;
        public FunnyEnum FunEnum { get; set; } = FunnyEnum.Fun;

        [JsonIgnore]
        public Vector2 Point { get; set; } = Vector2.up + Vector2.right;
        public SerializableVector2 SerializablePoint
        {
            get
            {
                return new() { x = Point.x, y = Point.y };
            }
            set
            {
                Point = new Vector2(value.x, value.y);
            }
        }
    }
    public struct SerializableVector2
    {
        public float x, y;
        public SerializableVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        
    }

    public enum FunnyEnum
    {
        NoFunAllowed,
        Unfun,
        Boring,
        Fun,
        SuperFun
    }
}
