using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Offworld.GameCore
{
    public class Fractal
    {
        const int VALUE_RANGE = 100;

        int miWidth = 0;
        int miHeight = 0;
        int miVariance = 0;
        int miResolution = 0;

        int[] maiValueCount = null;

        int[,] maaiValues = null;

        Random mRandom = null;

        public void init(int iSeed, int iWidth, int iHeight, int iVariance, int iCoefficient)
        {
            initHints(iSeed, iWidth, iHeight, iVariance, iCoefficient, 0, null);
        }

        void initHints(int iSeed, int iWidth, int iHeight, int iVariance, int iCoefficient, int iHintsCoefficient, int[,] aaiHints)
        {
            miWidth = iWidth;
            miHeight = iHeight;
            miVariance = iVariance;
            miResolution = (1 << iCoefficient) + 1;

            maiValueCount = new int[VALUE_RANGE + 1];
            for (int i = 0; i < VALUE_RANGE + 1; i++)
            {
                maiValueCount[i] = 0;
            }

            maaiValues = new int[miResolution, miResolution];
            for (int iX = 0; iX < miResolution; iX++)
            {
                for (int iY = 0; iY < miResolution; iY++)
                {
                    maaiValues[iX, iY] = 0;
                }
            }

            mRandom = new CrossPlatformRandom(iSeed);

            if (aaiHints == null)
            {
                maaiValues[0, 0] = mRandom.Next(VALUE_RANGE);
                maaiValues[miResolution - 1, 0] = mRandom.Next(VALUE_RANGE);
                maaiValues[0, miResolution - 1] = mRandom.Next(VALUE_RANGE);
                maaiValues[miResolution - 1, miResolution - 1] = mRandom.Next(VALUE_RANGE);
            }
            else
            {
                int iStep = 1 << (iCoefficient - iHintsCoefficient);

                for (int iX = 0; iX < miResolution; iX += iStep)
                {
                    for (int iY = 0; iY < miResolution; iY += iStep)
                    {
                        maaiValues[iX, iY] = aaiHints[iX / iStep, iX / iStep];
                    }
                }
            }

            for (int iLevel = iHintsCoefficient; iLevel < iCoefficient; iLevel++)
            {
                int iStep = 1 << (iCoefficient - iLevel);

                for (int iX = 0; iX < miResolution - 1; iX += iStep)
                {
                    for (int iY = 0; iY < miResolution - 1; iY += iStep)
                    {
                        diamond(iX, iY, iStep);
                        square(iX, iY, iStep);
                    }
                }
            }

            {
                int iMaxValue = int.MinValue;
                int iMinValue = int.MaxValue;

                for (int iX = 0; iX < miResolution; iX++)
                {
                    for (int iY = 0; iY < miResolution; iY++)
                    {
                        int iValue = maaiValues[iX, iY];

                        if (iValue > iMaxValue)
                        {
                            iMaxValue = iValue;
                        }
                        if (iValue < iMinValue)
                        {
                            iMinValue = iValue;
                        }
                    }
                }

                int iValueRange = (iMaxValue - iMinValue);

                for (int iX = 0; iX < miResolution; iX++)
                {
                    for (int iY = 0; iY < miResolution; iY++)
                    {
                        int iValue = (((maaiValues[iX, iY] - iMinValue) * VALUE_RANGE) / iValueRange);

                        maaiValues[iX, iY] = iValue;
                        maiValueCount[iValue]++;
                    }
                }
            }
        }

        public int getTileValue(int iX, int iY)
        {
            return maaiValues[(miResolution * iX) / miWidth, (miResolution * iY) / miHeight];
        }

        public int getValueByPercent(int iPercent)
        {
            int iTargetCount = ((miResolution * miResolution * iPercent) / 100);
            int iCount = 0;

            for (int i = 0; i < (VALUE_RANGE + 1); i++)
            {
                iCount += maiValueCount[i];

                if (iCount >= iTargetCount)
                {
                    return i;
                }
            }

            return VALUE_RANGE;
        }

        void diamond(int iX, int iY, int iStep)
        {
            if (iStep > 1)
            {
                int iAverage = ((maaiValues[iX, iY] + maaiValues[iX + iStep, iY] + maaiValues[iX, iY + iStep] + maaiValues[iX + iStep, iY + iStep]) + 2) / 4;
                int iHalf = (iStep / 2);
                maaiValues[iX + iHalf, iY + iHalf] = iAverage + mRandom.Next(miVariance) - (miVariance / 2);
            }
        }

        void sqaureQuarter(int iX, int iY, int iHalf)
        {
            int iValues = 0;
            int iCount = 0;

            if ((iX - iHalf) >= 0)
            {
                iValues += maaiValues[iX - iHalf, iY];
                iCount++;
            }

            if ((iX + iHalf) < miResolution)
            {
                iValues += maaiValues[iX + iHalf, iY];
                iCount++;
            }

            if ((iY - iHalf) >= 0)
            {
                iValues += maaiValues[iX, iY - iHalf];
                iCount++;
            }

            if ((iY + iHalf) < miResolution)
            {
                iValues += maaiValues[iX, iY + iHalf];
                iCount++;
            }

            maaiValues[iX, iY] = ((iValues + (iCount / 2)) / iCount);
        }

        void square(int iX, int iY, int iStep)
        {
            int iHalf = iStep / 2;

            sqaureQuarter(iX + iHalf, iY, iHalf);
            sqaureQuarter(iX, iY + iHalf, iHalf);
            sqaureQuarter(iX + iStep, iY + iHalf, iHalf);
            sqaureQuarter(iX + iHalf, iY + iStep, iHalf);
        }
    }
}