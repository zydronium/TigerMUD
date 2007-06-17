using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    class RandomNameGenerator
    {
        Randomizer random;


        public RandomNameGenerator()
        {
            char[] consonants ={ 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
            char[] vowels ={ 'a', 'e', 'i', 'o', 'u' };
            char[] letters ={ 'a', 'e', 'i', 'o', 'u', 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z' };
            random = new Randomizer();

        }

        public char GetChar()
        {
            // get random char, but tempered by english char usage frequency
            /* 
                E: 12.5% 875
                T: 9.0% 785
                A: 8.1% 704
                O: 7.7% 627
                N: 7.1% 556
                I: 6.8% 488
                H: 6.6% 422
                S: 6.3% 359
                R: 6.2% 297
                D: 4.7% 250
                L: 3.7% 213
                U: 2.8% 185
                M: 2.6% 159
                W: 2.4% 135
                C: 2.2% 113
                F: 2.2% 91
                G: 2.0% 71
                Y: 2.0% 51
                P: 1.6% 35
                B: 1.3% 22
                V: 0.8% 14
                K: 0.7% 7
                Q: 0.2% 5
                X: 0.2% 3
                J: 0.2% 1
                Z: 0.1% 0
                */

            int letterpercent = random.GetRandomNumber(0, 999);

            if (letterpercent < 1)
                return 'z';
            if (letterpercent < 3)
                return 'j';
            if (letterpercent < 5)
                return 'x';
            if (letterpercent < 7)
                return 'q';
            if (letterpercent < 14)
                return 'k';
            if (letterpercent < 22)
                return 'v';
            if (letterpercent < 35)
                return 'b';
            if (letterpercent < 51)
                return 'p';
            if (letterpercent < 71)
                return 'y';
            if (letterpercent < 91)
                return 'g';
            if (letterpercent < 113)
                return 'f';
            if (letterpercent < 135)
                return 'c';
            if (letterpercent < 159)
                return 'w';
            if (letterpercent < 185)
                return 'm';
            if (letterpercent < 213)
                return 'u';
            if (letterpercent < 250)
                return 'l';
            if (letterpercent < 297)
                return 'd';
            if (letterpercent < 359)
                return 'r';
            if (letterpercent < 422)
                return 's';
            if (letterpercent < 488)
                return 'h';
            if (letterpercent < 556)
                return 'i';
            if (letterpercent < 627)
                return 'n';
            if (letterpercent < 704)
                return 'o';
            if (letterpercent < 785)
                return 'a';
            if (letterpercent < 875)
                return 't';
            else
                //if (letterpercent < 1000)
                return 'e';


        }

        public char GetVowel()
        {
            /*  10000
              E: 32.98% 6702
              A: 21.40% 4562
              O: 20.24% 2538
              I: 17.97% 741
              U: 7.41% 0
             
             */

            int letterpercent = random.GetRandomNumber(0, 9999);

            if (letterpercent >= 6702 && letterpercent < 10000)
                return 'e';
            if (letterpercent >= 4562 && letterpercent < 6702)
                return 'a';
            if (letterpercent >= 2538 && letterpercent < 4562)
                return 'o';
            if (letterpercent >= 741 && letterpercent < 2538)
                return 'i';
            else
                return 'u';



        }

        public char GetConsonant()
        {
            /* 1000
               T: 14.5% 855
               N: 11.4% 741
               H: 10.6% 635
               S: 10.1% 534
               R: 9.98% 434
               D: 7.57% 359
               L: 5.96% 299
               M: 4.19% 257
               W: 3.86% 218
               C: 3.54% 183
               F: 3.52% 148
               G: 3.32% 115
               Y: 3.32% 81
               P: 2.58% 56
               B: 2.10% 35
               V: 1.20% 23
               K: 1.10% 12
               Q: .330% 8
               X: .330% 5
               J: .330% 2
               Z: .170% 0
               */

            int letterpercent = random.GetRandomNumber(0, 999);

            //if (letterpercent >= 855 && letterpercent < 1000)
            //    return 't';
            //if (letterpercent >= 741 && letterpercent < 855)
            //    return 'n';
            //if (letterpercent >= 635 && letterpercent < 741)
            //    return 'h';
            //if (letterpercent >= 534 && letterpercent < 635)
            //    return 's';
            //if (letterpercent >= 434 && letterpercent < 534)
            //    return 'r';
            //if (letterpercent >= 359 && letterpercent < 434)
            //    return 'd';
            //if (letterpercent >= 299 && letterpercent < 359)
            //    return 'l';
            //if (letterpercent >= 257 && letterpercent < 299)
            //    return 'm';
            //if (letterpercent >= 218 && letterpercent < 257)
            //    return 'w';
            //if (letterpercent >= 183 && letterpercent < 218)
            //    return 'c';
            //if (letterpercent >= 148 && letterpercent < 183)
            //    return 'f';
            //if (letterpercent >= 115 && letterpercent < 148)
            //    return 'g';
            //if (letterpercent >= 81 && letterpercent < 115)
            //    return 'y';
            //if (letterpercent >= 56 && letterpercent < 81)
            //    return 'p';
            //if (letterpercent >= 35 && letterpercent < 56)
            //    return 'b';
            //if (letterpercent >= 23 && letterpercent < 35)
            //    return 'v';
            //if (letterpercent >= 12 && letterpercent < 23)
            //    return 'k';
            //if (letterpercent >= 8 && letterpercent < 12)
            //    return 'q';
            //if (letterpercent >= 5 && letterpercent < 8)
            //    return 'x';
            //if (letterpercent >= 2 && letterpercent < 5)
            //    return 'j';
            //else
            //    return 'z';

            if (letterpercent < 2)
                return 'z';
            if (letterpercent < 5)
                return 'j';
            if (letterpercent < 8)
                return 'x';
            if (letterpercent < 12)
                return 'q';
            if (letterpercent < 23)
                return 'k';
            if (letterpercent < 35)
                return 'v';
            if (letterpercent < 56)
                return 'b';
            if (letterpercent < 81)
                return 'p';
            if (letterpercent < 115)
                return 'y';
            if (letterpercent < 148)
                return 'g';
            if (letterpercent < 183)
                return 'f';
            if (letterpercent < 218)
                return 'c';
            if (letterpercent < 257)
                return 'w';
            if (letterpercent < 299)
                return 'm';
            if (letterpercent < 359)
                return 'l';
            if (letterpercent < 434)
                return 'd';
            if (letterpercent < 534)
                return 'r';
            if (letterpercent < 635)
                return 's';
            if (letterpercent < 741)
                return 'h';
            if (letterpercent < 855)
                return 'n';
            //if (letterpercent < 1000)
            else
                return 't';






        }


    }
}
