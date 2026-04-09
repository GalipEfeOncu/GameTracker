using System;
using GameTracker.Models;

namespace GameTracker.Services;

/// <summary>Plan §3.4: Metacritic yoksa sırayla en iyi mümkün puan + etiket.</summary>
public static class GameDisplayScoreHelper
{
    public const string KindMetacritic = "metacritic";
    public const string KindIgdbCriticAggregate = "igdb_critic_aggregate";
    public const string KindIgdbTotal = "igdb_total";
    public const string KindNone = "none";

    public static void Apply(
        Game game,
        int? rawgMetacritic,
        double? igdbAggregatedRating,
        int? igdbAggregatedRatingCount,
        double? igdbTotalRating)
    {
        game.Metacritic = rawgMetacritic is > 0 ? rawgMetacritic : null;

        if (igdbAggregatedRating is > 0 && igdbAggregatedRatingCount is > 0)
        {
            game.IgdbAggregatedRating = Math.Round(igdbAggregatedRating.Value, 1);
            game.IgdbAggregatedRatingCount = igdbAggregatedRatingCount;
        }
        else
        {
            game.IgdbAggregatedRating = null;
            game.IgdbAggregatedRatingCount = null;
        }

        game.IgdbTotalRating = igdbTotalRating is > 0
            ? Math.Round(igdbTotalRating.Value, 1)
            : null;

        if (rawgMetacritic is > 0)
        {
            game.DisplayScore = rawgMetacritic;
            game.DisplayScoreKind = KindMetacritic;
            game.DisplayScoreLabel = "Metacritic";
            return;
        }

        if (igdbAggregatedRating is > 0 && igdbAggregatedRatingCount is > 0)
        {
            game.DisplayScore = game.IgdbAggregatedRating;
            game.DisplayScoreKind = KindIgdbCriticAggregate;
            game.DisplayScoreLabel = "Eleştiri özeti (IGDB)";
            return;
        }

        if (igdbTotalRating is > 0)
        {
            game.DisplayScore = game.IgdbTotalRating;
            game.DisplayScoreKind = KindIgdbTotal;
            game.DisplayScoreLabel = "Toplam puan (IGDB)";
            return;
        }

        game.DisplayScore = null;
        game.DisplayScoreKind = KindNone;
        game.DisplayScoreLabel = null;
    }
}
