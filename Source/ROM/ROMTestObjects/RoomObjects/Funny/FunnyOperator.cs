﻿using Newtonsoft.Json.Linq;
using ROM.RoomObjectService;
using ROM.UserInteraction;
using ROM.UserInteraction.InroomManagement;
using ROM.UserInteraction.ObjectEditorElement;
using ROM.UserInteraction.ObjectEditorElement.Scrollbar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROMTestObjects.RoomObjects.Funny
{
    internal class FunnyOperator : TypeOperator<FunnyObject>
    {
        private static VersionedLoader<FunnyObject> VersionedLoader { get; } =
            TypeOperatorUtils.CreateVersionedLoader(defaultLoad: TypeOperatorUtils.TrivialLoad<FunnyObject>);

        public override string TypeId => nameof(FunnyObject);

        public override FunnyObject CreateNew(Room room)
        {
            return new() { room = room };
        }

        public override FunnyObject Load(JToken dataJson, Room room)
        {
            return VersionedLoader.Load(dataJson, room);
        }

        public override JToken Save(FunnyObject obj)
        {
            return TypeOperatorUtils.GetTrivialVersionedSaveCall<FunnyObject>("0.0.0").Invoke(obj);
        }

        public override void AddToRoom(FunnyObject obj, Room room)
        {
            room.AddObject(obj);
        }

        public override void RemoveFromRoom(FunnyObject obj, Room room)
        {
            room.RemoveObject(obj);
        }

        public override IEnumerable<IObjectEditorElement> GetEditorElements(FunnyObject obj, Room room)
        {
            //yield return Elements.TextField(nameof(FunnyObject.JustAFloat),
            //    getter: () => obj.JustAFloat, setter: value => obj.JustAFloat = value);

            //yield return Elements.TextField(nameof(FunnyObject.JustAnInt),
            //    getter: () => obj.JustAnInt, setter: value => obj.JustAnInt = value);

            //yield return Elements.Scrollbar(nameof(FunnyObject.NormFloat),
            //    getter: () => obj.NormFloat, setter: value => obj.NormFloat = value);

            //FloatScrollbarConfiguration plusMinusTenFloat = new()
            //{
            //    Left = -10,
            //    Right = 10,
            //};

            //yield return Elements.Scrollbar(nameof(FunnyObject.PlusMinusTenFloat),
            //    getter: () => obj.PlusMinusTenFloat, setter: value => obj.PlusMinusTenFloat = value,
            //    configuration: plusMinusTenFloat);

            yield return Elements.Scrollbar(nameof(FunnyObject.PlusFiveMinusThreeInt),
                getter: () => obj.PlusFiveMinusThreeInt, setter: value => obj.PlusFiveMinusThreeInt = value,
                left: 5, right: -3);

            yield return Elements.Scrollbar(nameof(FunnyObject.PlusFiveMinusThreeInt2),
                getter: () => obj.PlusFiveMinusThreeInt2, setter: value => obj.PlusFiveMinusThreeInt2 = value,
                left: -3, right: 5);

            //List<Option<char>> charOptions = [
            //    new Option<char>('a', "a"),
            //    new Option<char>('b', "b"),
            //    new Option<char>('c', "c"),
            //    new Option<char>('d', "d"),
            //    new Option<char>('e', "e")
            //    ];

            //yield return Elements.Scrollbar(nameof(FunnyObject.CursedChar),
            //    getter: () => obj.CursedChar, setter: value => obj.CursedChar = value,
            //    charOptions);
        }
    }
}