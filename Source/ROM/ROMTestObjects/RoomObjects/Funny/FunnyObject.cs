using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
