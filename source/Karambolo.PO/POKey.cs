using System;
using System.Collections.Generic;

namespace Karambolo.PO
{
    public enum POIdKind
    {
        Unknown,
        Id,
        PluralId,
        ContextId,
    }

    public struct POKey : IEquatable<POKey>
    {
        internal static POIdKind GetIdKind(string value)
        {
            switch (value)
            {
                case POCatalog.IdToken: return POIdKind.Id;
                case POCatalog.PluralIdToken: return POIdKind.PluralId;
                case POCatalog.ContextIdToken: return POIdKind.ContextId;
                default: return POIdKind.Unknown;
            }
        }

        internal static string GetIdKindToken(POIdKind value)
        {
            switch (value)
            {
                case POIdKind.Id: return POCatalog.IdToken;
                case POIdKind.PluralId: return POCatalog.PluralIdToken;
                case POIdKind.ContextId: return POCatalog.ContextIdToken;
                default: return null;
            }
        }

        public static bool operator ==(POKey left, POKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(POKey left, POKey right)
        {
            return !left.Equals(right);
        }

        public POKey(string id, string pluralId = null, string contextId = null)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            Id = id;
            PluralId = pluralId;
            ContextId = contextId;
        }

        public string Id { get; }
        public string PluralId { get; }
        public string ContextId { get; }

        public bool IsValid => Id != null;

        internal bool IsHeaderEntryKey => IsValid && Id.Length == 0 && PluralId == null && ContextId == null;

        public bool Equals(POKey other)
        {
            return
                Id == other.Id &&
                PluralId == other.PluralId &&
                ContextId == other.ContextId;
        }

        public override bool Equals(object obj)
        {
            return obj is POKey key && Equals(key);
        }

        public override int GetHashCode()
        {
            EqualityComparer<string> stringComparer = EqualityComparer<string>.Default;
            int hashCode = -1618513021;
            hashCode = hashCode * -1521134295 + stringComparer.GetHashCode(Id);
            hashCode = hashCode * -1521134295 + stringComparer.GetHashCode(PluralId);
            hashCode = hashCode * -1521134295 + stringComparer.GetHashCode(ContextId);
            return hashCode;
        }

        public override string ToString()
        {
            return IsValid ? Id : "(invalid)";
        }
    }
}
