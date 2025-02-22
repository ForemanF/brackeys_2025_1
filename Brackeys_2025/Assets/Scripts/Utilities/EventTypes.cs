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

public class NextTurnEvent
{

    public NextTurnEvent() {
    }

}
