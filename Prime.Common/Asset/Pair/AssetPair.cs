using System;
using System.Collections.Generic;
using System.Linq;
using Prime.Common;
using Prime.Utility;

namespace Prime.Common
{
    public class AssetPair : IEquatable<AssetPair>
    {
        private AssetPair() { }

        /// <summary>
        /// The first listed currency of a currency pair is called the base currency, and the second currency is called the quote currency.
        /// The currency pair indicates how much of the quote currency is needed to purchase one unit of the base currency.
        /// </summary>
        /// <param name="asset1">Base Currency</param>
        /// <param name="asset2">Quote Currency</param>
        public AssetPair(Asset asset1, Asset asset2)
        {
            Asset1 = asset1 ?? throw new ArgumentException($"{nameof(asset1)} is null.");
            Asset2 = asset2 ?? throw new ArgumentException($"{nameof(asset2)} is null.");
        }

        public AssetPair(string asset1, string asset2) : this(asset1.ToAssetRaw(), asset2.ToAssetRaw())
        {
        }

        public AssetPair(string asset1, string asset2, IDescribesAssets provider) : this(Assets.I.Get(asset1, provider), Assets.I.Get(asset2, provider))
        {
        }

        /// <summary>
        /// Base currency
        /// </summary>
        [Bson]
        public Asset Asset1 { get; private set; }

        /// <summary>
        /// Quote currency. 
        /// </summary>
        [Bson]
        public Asset Asset2 { get; private set; }

        public bool IsEmpty => Asset1 == Asset.None || Asset2 == Asset.None;

        private bool? _isNormalised;
        public bool IsNormalised => _isNormalised ?? (bool)(_isNormalised = string.CompareOrdinal(Asset1.ShortCode, Asset2.ShortCode) < 0);

        private AssetPair _normalised;
        public AssetPair Normalised => _normalised ?? (_normalised = IsNormalised ? this : Reverse());

        public AssetPair Reverse() => new AssetPair(Asset2, Asset1);

        public static AssetPair Empty => new AssetPair(Asset.None, Asset.None);

        public string TickerDash()
        {
            return $"{Asset1.ShortCode}-{Asset2.ShortCode}";
        }

        public string TickerUnderslash()
        {
            return $"{Asset1.ShortCode}_{Asset2.ShortCode}";
        }

        public string TickerSimple()
        {
            return $"{Asset1.ShortCode}{Asset2.ShortCode}";
        }

        public bool Equals(AssetPair other)
        {
            return Asset1.Equals(other.Asset1) && Asset2.Equals(other.Asset2);
        }

        public bool Equals(AssetPair other, bool eitherDirection)
        {
            if (!eitherDirection)
                return Equals(other);

            return Equals(other) || (Asset1.Equals(other.Asset2) && Asset2.Equals(other.Asset1));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AssetPair && Equals((AssetPair) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Asset1.GetHashCode() * 397) ^ Asset2.GetHashCode();
            }
        }

        public override string ToString()
        {
            return Asset1 + ":" + Asset2;
        }

        public static implicit operator AssetPair(string value)
        {
            var p = value.Split(':');
            return p.Length == 2 ? new AssetPair(Assets.I.GetRaw(p[0]), Assets.I.GetRaw(p[1])) : null;
        }

        private UniqueList<AssetPair> _altPairs;
        public IReadOnlyList<AssetPair> PeggedPairs => _altPairs ?? (_altPairs = GetAltPairs());

        private UniqueList<AssetPair> GetAltPairs()
        {
            if (Asset2.Pegged.Count == 0)
                return new UniqueList<AssetPair>();

            var r = new UniqueList<AssetPair>();
            foreach (var i in Asset2.Pegged)
                r.Add(new AssetPair(Asset1, i));

            return r;
        }
    }
}