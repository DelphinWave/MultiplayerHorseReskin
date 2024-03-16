using System;

namespace MultiplayerHorseReskin.Framework
{
    class HorseReskinMessage
    {
        public Guid horseId;
        public string skinId;
        public HorseReskinMessage(Guid horseId, string skinId)
        {
            this.horseId = horseId;
            this.skinId = skinId;
        }
    }
}
