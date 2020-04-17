using System;

namespace Company.Gazoo.Utils
{
    public static class StringDistance
    {
        public static double CountWordErrorRate(string transcription, string verification)
        {
            if(string.IsNullOrEmpty(transcription) && string.IsNullOrEmpty(verification))
                return 0;

            transcription = transcription ?? string.Empty;
            verification = verification ?? string.Empty;

            var tr = transcription.Split(' ');
            var vr = verification.Split(' ');

            var trLength = string.IsNullOrEmpty(transcription) ? 0 : tr.Length;
            var vrLength = string.IsNullOrEmpty(verification) ? 0 : vr.Length;

            double deletion, substitution, insertion;
            double[,] distance = new double[trLength + 1, vrLength + 1];

            if (trLength == 0)
                return 100;

            if (vrLength == 0)
                return 100;

            for (var trIterator = 0; trIterator <= trLength; trIterator++)
            {
                distance[trIterator, 0] = trIterator;
            }

            for (var vrIterator = 0; vrIterator <= vrLength; vrIterator++)
            {
                distance[0, vrIterator] = vrIterator;
            }

            for (var trIterator = 1; trIterator <= trLength; trIterator++)
            {
                for (var vrIterator = 1; vrIterator <= vrLength; vrIterator++)
                {
                    if (vr[vrIterator - 1] == tr[trIterator - 1])
                    {
                        distance[trIterator, vrIterator] = distance[trIterator - 1, vrIterator - 1];
                    }
                    else
                    {
                        substitution = distance[trIterator - 1, vrIterator - 1] + 1;
                        insertion = distance[trIterator, vrIterator - 1] + 1;
                        deletion = distance[trIterator - 1, vrIterator] + 1;
                        distance[trIterator, vrIterator] = Math.Min(substitution, Math.Min(insertion, deletion));
                    }
                }
            }

            return (distance[trLength, vrLength] / vrLength) * 100;
        }

        public static double CountCharErrorRate(string transcription, string verification)
        {
            if (string.IsNullOrEmpty(transcription) && string.IsNullOrEmpty(verification))
                return 0;

            transcription = transcription ?? string.Empty;
            verification = verification ?? string.Empty;

            var trLength = transcription.Length;
            var vrLength = verification.Length;
            double[,] distance = new double[trLength + 1, vrLength + 1];

            if (trLength == 0)
                return 100;

            if (vrLength == 0)
                return 100;

            for (var trIterator = 0; trIterator <= trLength; trIterator++)
            {
                distance[trIterator, 0] = trIterator;
            }

            for (var vrIterator = 0; vrIterator <= vrLength; vrIterator++)
            {
                distance[0, vrIterator] = vrIterator;
            }

            for (var trIterator = 1; trIterator <= trLength; trIterator++)
            {
                for (var vrIterator = 1; vrIterator <= vrLength; vrIterator++)
                {
                    var cost = (verification[vrIterator - 1] == transcription[trIterator - 1]) ? 0 : 1;

                    distance[trIterator, vrIterator] =
                        Math.Min(
                            Math.Min(distance[trIterator - 1, vrIterator] + 1,
                            distance[trIterator, vrIterator - 1] + 1),
                            distance[trIterator - 1, vrIterator - 1] + cost
                        );
                }
            }

            return (distance[trLength, vrLength] / vrLength) * 100;
        }
    }
}
