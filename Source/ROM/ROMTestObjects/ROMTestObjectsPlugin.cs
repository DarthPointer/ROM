using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using ROM.RoomObjectService;
using ROMTestObjects.RoomObjects.Funny;
using ROM.RoomObjectService.SpawningCondition;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618

namespace ROMTestObjects
{
    [BepInPlugin(GUID: "DarthPointer.ROMTestObjects", Name: "ROMTestObjects", Version: "0.0.0")]
    public class ROMTestObjectsPlugin : BaseUnityPlugin
    {
        public void OnEnable()
        {
            TypeOperator.RegisterType<FunnyOperator>();
        }
    }
}
