﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Enables LINQ queries, which will be explained soon

// The player can either be human or an AI
public enum PlayerType
{
    human,
    ai
}

[System.Serializable]
public class Player {
    public PlayerType type = PlayerType.human;
    public int playerNum;
    public SlotDef handSlotDef;
    public List<CardLostCities> hand; // The cards in this player's hand

	// Add a card to the hand
    public CardLostCities AddCard(CardLostCities eCB)
    {
        if (hand == null) hand = new List<CardLostCities>();

        // Add the card to the hand
        hand.Add(eCB);

        //Sort the cards by rank using LINQ if this is a human
        if (type == PlayerType.human)
        {
            CardLostCities[] cards = hand.ToArray();

            // This is the LINQ call
            cards = cards.OrderBy(cd => cd.rank).ToArray();
            

            hand = new List<CardLostCities>(cards);
            // Note: LINQ operations can be a bit slow (like it could take a
            // couple of milliseconds), but since we're only doing it once
            // every round, it isn't a problem.
        }

        eCB.SetSortingLayerName("10"); // Sorts the moving card to the top
        eCB.eventualSortLayer = handSlotDef.layerName;

        FanHand();
        return (eCB);
    }

    // Remove a card from the hand
    public CardLostCities RemoveCard(CardLostCities cb)
    {
        // If hand is null or doesn't contain cb, return null
        if (hand == null || !hand.Contains(cb)) return null;
        hand.Remove(cb);
        FanHand();
        return (cb);
    }
    public void CardRotate()
    {
        Vector3 pos;
        pos = Vector3.up * CardLostCities.CARD_HEIGHT / 2f;
        Quaternion rotQ;
        rotQ = Quaternion.Euler(0, 0, 180);
        hand[1].MoveTo(pos, rotQ);
    }
    public void FanHand()
    {
        // startRot is the rotation about Z of the first card
        
        float startRot = 0;
        startRot = handSlotDef.rot;
        if(hand.Count > 1)
        {
            startRot += LostCities.S.handFanDegrees * (hand.Count - 1) / 2;
        }

        // Move all the cards to their new positions
        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for (int i=0; i<hand.Count; i++)
        {
            rot = startRot - LostCities.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);

            pos = Vector3.up * CardLostCities.CARD_HEIGHT / 2f;

            pos = rotQ * pos;

            // Add the base position of the player's hand (which will be at the
            // bottom-center of the fan of the cards)
            pos += handSlotDef.pos;
            //    pos.z = -0.5f * i;
            Debug.Log(i);
            
            pos.x =2.75f*i - 9.5f;
            

            
            // If not the initial deal, start moving the card immediately.
            if(LostCities.S.phase != TurnPhase.idle)
            {
                hand[i].timeStart = 0;
            }
            
            // Set the localPosition and rotation of the ith card in the hand
            hand[i].MoveTo(pos, rotQ); // Tell CardLostCities to interpolate
            hand[i].state = CBState.toHand;
            // After the move, CardLostCities will set the state to CBState.hand

            /* <= This begins a multiline comment
            hand[i].transform.localPosition = pos;
            hand[i].transform.rotation = rotQ;
            hand[i].state = CBState.hand; 
            This ends the multiline comment => */
            if(type == PlayerType.human)
            {
                hand[i].faceUp = (type == PlayerType.human);
            }
            else if (type == PlayerType.ai)
            {
                hand[i].faceUp = (type == PlayerType.ai);
            }

            // Set the SortOrder of the cards so that they overlap properly
            hand[i].eventualSortOrder = i * 4;
            //hand[i].SetSortOrder(i * 4);
        }
    }

    // The TakeTurn() function enables the AI of the computer Players
    public void TakeTurn()
    {
        Utils.tr("Player.TakeTurn");

        // Don't need to do anything if this is the human player.
        if (type == PlayerType.human) return;

        LostCities.S.phase = TurnPhase.waiting;

        CardLostCities cb;

        // If this is an AI player, need to make a choice about what to play
        // Find valid plays
        List<CardLostCities> validCards = new List<CardLostCities>();
        foreach (CardLostCities tCB in hand)
        {
            if (LostCities.S.ValidPlay(tCB))
            {
                validCards.Add(tCB);
            }
        }
        // If there are no valid cards
        if(validCards.Count == 0)
        {
            // ... then draw a card
            cb = AddCard(LostCities.S.Draw());
            cb.callbackPlayer = this;
            return;
        }

        // So, there is a card or more to play, so pick one
        cb = validCards[Random.Range(0, validCards.Count)];
        RemoveCard(cb);
        LostCities.S.MoveToTarget(cb);
        cb.callbackPlayer = this;
    }

    public void CBCallback(CardLostCities tCB)
    {
        Utils.tr("Player.CBCallback()", tCB.name, "Player " + playerNum);
        // The card is done moving, so pass the turn
        LostCities.S.PassTurn();
    }
}
