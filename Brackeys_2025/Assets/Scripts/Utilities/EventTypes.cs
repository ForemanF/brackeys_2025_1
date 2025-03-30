using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HighlightTileEvent {
    public HexTile hex_tile;

    public HighlightTileEvent(HexTile _hex_tile) {
        hex_tile = _hex_tile;
    }
}

public class HighlightCardActionEvent {
    public HexTile hex_tile;
    public Card card;

    public HighlightCardActionEvent(HexTile _hex_tile, Card _card) {
        hex_tile = _hex_tile;
        card = _card;
    }
}

public class TakeCardActionEvent
{
    public HexTile hex_tile;
    public Card card;

    public TakeCardActionEvent(HexTile _hex_tile, Card _card)
    {
        hex_tile = _hex_tile;
        card = _card;
    }
}

public class DiscardCardEvent
{
    public Card discarded_card;

    public DiscardCardEvent(Card _discarded_card) {
        discarded_card = _discarded_card;
    }

}

public class RevealTileEvent
{
    public HexTile revealed_tile;

    public RevealTileEvent(HexTile _revealed_tile) {
        revealed_tile = _revealed_tile;
    }
}

public class ParticleBurstEvent
{
    public Vector3 position;
    public BurstType burst_type;
    public int emit_amt;

    public ParticleBurstEvent(Vector3 _position, BurstType _burst_type, int _emit_amt) {
        position = _position;
        burst_type = _burst_type;
        emit_amt = _emit_amt;
    }
}

public class DestroyedFactionObjectEvent
{
    public Faction faction;
    public GameObject game_object;

    public DestroyedFactionObjectEvent(Faction _faction, GameObject _game_object) {
        faction = _faction;
        game_object = _game_object;
    }
}

public class CreateBuildingEvent
{
    public BuildingCard building_card;
    public HexTile hex_tile;

    public CreateBuildingEvent(BuildingCard _building_card, HexTile _hex_tile) {
        building_card = _building_card;
        hex_tile = _hex_tile;
    }
}

public class ResetTileEvent
{
    public HexTile hex_tile;

    public ResetTileEvent(HexTile _hex_tile) {
        hex_tile = _hex_tile;
    }
}

public class IncreaseEnergyEvent
{
    public int energy_increase;

    public IncreaseEnergyEvent(int _energy_increase) {
        energy_increase = _energy_increase;
    }
}

public class DrawCardEvent
{
    public int amount;

    public DrawCardEvent(int _amount) {
        amount = _amount;
    }
}

public class SpawnSoldierEvent
{
    public HexTile location;
    public GameObject soldier_prefab;

    public SpawnSoldierEvent(HexTile _location, GameObject _soldier_prefab) {
        location = _location;
        soldier_prefab = _soldier_prefab;
    }
}

public class NextTurnEvent
{
    public NextTurnEvent() {}
}

public class RevealNearestFogEvent
{
    public HexTile source_tile;
    public int amount;

    public RevealNearestFogEvent(HexTile _source_tile, int _amount) {
        source_tile = _source_tile;
        amount = _amount;
    }
}

public class AudioEvent
{
    public AudioType audio_type;
    public Vector3 position;
    public float volume;

    public AudioEvent(AudioType _audio_type, Vector3 _position, float _volume = 1) {
        audio_type = _audio_type;
        position = _position;
        volume = _volume;
    }
}


