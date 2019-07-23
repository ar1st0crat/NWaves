using System;
using System.Linq;

namespace NWaves.Transforms.Wavelets
{
    /// <summary>
    /// Wavelet
    /// </summary>
    public class Wavelet
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The length of the mother wavelet
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// HP coefficients for decomposition
        /// </summary>
        public float[] LoD { get; set; }

        /// <summary>
        /// HP coefficients for decomposition
        /// </summary>
        public float[] HiD { get; set; }

        /// <summary>
        /// LP coefficients for reconstruction
        /// </summary>
        public float[] LoR { get; set; }

        /// <summary>
        /// HP coefficients for reconstruction
        /// </summary>
        public float[] HiR { get; set; }

        /// <summary>
        /// Constructor from wavelet family and number of taps
        /// </summary>
        /// <param name="waveletFamily"></param>
        /// <param name="taps">Set for all wavelets</param>
        public Wavelet(WaveletFamily waveletFamily, int taps = 1)
        {
            MakeWavelet(waveletFamily, taps);
        }

        /// <summary>
        /// Constructor from name
        /// </summary>
        /// <param name="name"></param>
        public Wavelet(string name)
        {
            WaveletFamily waveletFamily;
            int taps = 1;

            name = name.ToLower();

            if (name == "haar")
            {
                waveletFamily = WaveletFamily.Haar;
            }
            else
            {
                var digitPos = -1;
                for (var i = 0; i < name.Length; i++)
                {
                    if (char.IsDigit(name[i]))
                    {
                        digitPos = i;
                        break;
                    }
                }

                var wname = name;

                if (digitPos < 0)
                {
                    taps = 1;
                }
                else
                {
                    wname = name.Substring(0, digitPos);
                    taps = int.Parse(name.Substring(digitPos));
                }

                switch (wname)
                {
                    case "db":
                        waveletFamily = WaveletFamily.Daubechies;
                        break;
                    case "sym":
                        waveletFamily = WaveletFamily.Symlet;
                        break;
                    case "coif":
                        waveletFamily = WaveletFamily.Coiflet;
                        break;
                    default:
                        throw new ArgumentException($"Unrecognized wavelet name: {name}");
                }
            }

            MakeWavelet(waveletFamily, taps);
        }

        /// <summary>
        /// Fill wavelet fields: name, length and coefficients
        /// </summary>
        /// <param name="waveletFamily"></param>
        /// <param name="taps"></param>
        private void MakeWavelet(WaveletFamily waveletFamily, int taps)
        {
            switch (waveletFamily)
            {
                case WaveletFamily.Daubechies:
                    MakeDaubechiesWavelet(taps);
                    break;
                case WaveletFamily.Symlet:
                    MakeSymletWavelet(taps);
                    break;
                case WaveletFamily.Coiflet:
                    MakeCoifletWavelet(taps);
                    break;
                default:
                    MakeHaarWavelet();
                    break;
            }

            ComputeOrthonormalCoeffs();
        }

        /// <summary>
        /// Compute orthonormal coefficients from LoD coefficients only
        /// </summary>
        public void ComputeOrthonormalCoeffs()
        {
            HiD = LoD.Reverse().ToArray();

            for (var i = 0; i < HiD.Length; i += 2)
            {
                HiD[i] = -HiD[i];
            }

            LoR = LoD.Reverse().ToArray();
            HiR = HiD.Reverse().ToArray();
        }


        #region wavelet coefficients

        /// <summary>
        /// Haar wavelet
        /// </summary>
        public void MakeHaarWavelet()
        {
            Name = "haar";
            Length = 2;

            var sqrt2 = (float)Math.Sqrt(2);

            LoD = new[] { 1 / sqrt2, 1 / sqrt2 };
        }

        /// <summary>
        /// Daubechies wavelet
        /// </summary>
        /// <param name="taps"></param>
        public void MakeDaubechiesWavelet(int taps)
        {
            Name = $"db{taps}";
            Length = 2 * taps;

            switch (taps)
            {
                case 1:
                    var sqrt2 = (float)Math.Sqrt(2);            // just like Haar
                    LoD = new[] { 1 / sqrt2, 1 / sqrt2 };
                    break;
                case 2:
                    LoD = new[] { -0.12940952255092145f,
                                   0.22414386804185735f,
                                   0.836516303737469f,
                                   0.48296291314469025f };
                    break;
                case 3:
                    LoD = new[] { 0.035226291882100656f,
                                 -0.08544127388224149f,
                                 -0.13501102001039084f,
                                  0.4598775021193313f,
                                  0.8068915093133388f,
                                  0.3326705529509569f };
                    break;
                case 4:
                    LoD = new[] { -0.010597401784997278f,
                                   0.032883011666982945f,
                                   0.030841381835986965f,
                                  -0.18703481171888114f,
                                  -0.02798376941698385f,
                                   0.6308807679295904f,
                                   0.7148465705525415f,
                                   0.23037781330885523f };
                    break;
                case 5:
                    LoD = new[] { 0.003335725285001549f,
                                 -0.012580751999015526f,
                                 -0.006241490213011705f,
                                  0.07757149384006515f,
                                 -0.03224486958502952f,
                                 -0.24229488706619015f,
                                  0.13842814590110342f,
                                  0.7243085284385744f,
                                  0.6038292697974729f,
                                  0.160102397974125f };
                    break;
                case 6:
                    LoD = new[] { -0.00107730108499558f,
                                   0.004777257511010651f,
                                   0.0005538422009938016f,
                                  -0.031582039318031156f,
                                   0.02752286553001629f,
                                   0.09750160558707936f,
                                  -0.12976686756709563f,
                                  -0.22626469396516913f,
                                   0.3152503517092432f,
                                   0.7511339080215775f,
                                   0.4946238903983854f,
                                   0.11154074335008017f };
                    break;
                case 7:
                    LoD = new[] { 0.0003537138000010399f,
                                 -0.0018016407039998328f,
                                  0.00042957797300470274f,
                                  0.012550998556013784f,
                                 -0.01657454163101562f,
                                 -0.03802993693503463f,
                                  0.0806126091510659f,
                                  0.07130921926705004f,
                                 -0.22403618499416572f,
                                 -0.14390600392910627f,
                                  0.4697822874053586f,
                                  0.7291320908465551f,
                                  0.39653931948230575f,
                                  0.07785205408506236f };
                    break;
                case 8:
                    LoD = new[] { -0.00011747678400228192f,
                                   0.0006754494059985568f,
                                  -0.0003917403729959771f,
                                  -0.00487035299301066f,
                                   0.008746094047015655f,
                                   0.013981027917015516f,
                                  -0.04408825393106472f,
                                  -0.01736930100202211f,
                                   0.128747426620186f,
                                   0.00047248457399797254f,
                                  -0.2840155429624281f,
                                  -0.015829105256023893f,
                                   0.5853546836548691f,
                                   0.6756307362980128f,
                                   0.3128715909144659f,
                                   0.05441584224308161f };
                    break;
                case 9:
                    LoD = new[] { 3.9347319995026124e-05f,
                                 -0.0002519631889981789f,
                                  0.00023038576399541288f,
                                  0.0018476468829611268f,
                                 -0.004281503681904723f,
                                 -0.004723204757894831f,
                                  0.022361662123515244f,
                                  0.00025094711499193845f,
                                 -0.06763282905952399f,
                                  0.030725681478322865f,
                                  0.14854074933476008f,
                                 -0.09684078322087904f,
                                 -0.29327378327258685f,
                                  0.13319738582208895f,
                                  0.6572880780366389f,
                                  0.6048231236767786f,
                                  0.24383467463766728f,
                                  0.03807794736316728f };
                    break;
                case 10:
                    LoD = new[] { -1.326420300235487e-05f,
                                   9.358867000108985e-05f,
                                  -0.0001164668549943862f,
                                  -0.0006858566950046825f,
                                   0.00199240529499085f,
                                   0.0013953517469940798f,
                                  -0.010733175482979604f,
                                   0.0036065535669883944f,
                                   0.03321267405893324f,
                                  -0.02945753682194567f,
                                  -0.07139414716586077f,
                                   0.09305736460380659f,
                                   0.12736934033574265f,
                                  -0.19594627437659665f,
                                  -0.24984642432648865f,
                                   0.2811723436604265f,
                                   0.6884590394525921f,
                                   0.5272011889309198f,
                                   0.18817680007762133f,
                                   0.026670057900950818f };
                    break;
                case 11:
                    LoD = new[] { 4.494274277236352e-06f,
                                -3.463498418698379e-05f,
                                5.443907469936638e-05f,
                                0.00024915252355281426f,
                                -0.0008930232506662366f,
                                -0.00030859285881515924f,
                                0.004928417656058778f,
                                -0.0033408588730145018f,
                                -0.015364820906201324f,
                                0.02084090436018004f,
                                0.03133509021904531f,
                                -0.06643878569502022f,
                                -0.04647995511667613f,
                                0.14981201246638268f,
                                0.06604358819669089f,
                                -0.27423084681792875f,
                                -0.16227524502747828f,
                                0.41196436894789695f,
                                0.6856867749161785f,
                                0.44989976435603013f,
                                0.1440670211506196f,
                                0.01869429776147044f };
                    break;
                case 12:
                    LoD = new[] { -1.5290717580684923e-06f,
                                1.2776952219379579e-05f,
                                -2.4241545757030318e-05f,
                                -8.850410920820318e-05f,
                                0.0003886530628209267f,
                                6.5451282125215034e-06f,
                                -0.0021795036186277044f,
                                0.0022486072409952287f,
                                0.006711499008795549f,
                                -0.012840825198299882f,
                                -0.01221864906974642f,
                                0.04154627749508764f,
                                0.010849130255828966f,
                                -0.09643212009649671f,
                                0.0053595696743599965f,
                                0.18247860592758275f,
                                -0.023779257256064865f,
                                -0.31617845375277914f,
                                -0.04476388565377762f,
                                0.5158864784278007f,
                                0.6571987225792911f,
                                0.3773551352142041f,
                                0.10956627282118277f,
                                0.013112257957229239f };
                    break;

                default:
                    throw new ArgumentException("Only db1-db20 are supported");
            }
        }

        /// <summary>
        /// Symlet wavelet
        /// </summary>
        /// <param name="taps"></param>
        public void MakeSymletWavelet(int taps)
        {
            Name = $"sym{taps}";
            Length = 2 * taps;

            switch (taps)
            {
                case 2:
                    LoD = new[] { -0.12940952255092145f,
                                   0.22414386804185735f,
                                   0.836516303737469f,
                                   0.48296291314469025f };
                    break;
                case 3:
                    LoD = new[] { 0.035226291882100656f,
                                 -0.08544127388224149f,
                                 -0.13501102001039084f,
                                  0.4598775021193313f,
                                  0.8068915093133388f,
                                  0.3326705529509569f };
                    break;
                case 4:
                    LoD = new[] { -0.07576571478927333f,
                                  -0.02963552764599851f,
                                   0.49761866763201545f,
                                   0.8037387518059161f,
                                   0.29785779560527736f,
                                  -0.09921954357684722f,
                                  -0.012603967262037833f,
                                   0.0322231006040427f };
                    break;
                case 5:
                    LoD = new[] { 0.027333068345077982f,
                                  0.029519490925774643f,
                                 -0.039134249302383094f,
                                  0.1993975339773936f,
                                  0.7234076904024206f,
                                  0.6339789634582119f,
                                  0.01660210576452232f,
                                 -0.17532808990845047f,
                                 -0.021101834024758855f,
                                  0.019538882735286728f };
                    break;
                default:
                    throw new ArgumentException("Only sym2-sym20 are supported");
            }
        }

        /// <summary>
        /// Coiflet wavelet
        /// </summary>
        /// <param name="taps"></param>
        public void MakeCoifletWavelet(int taps)
        {
            Name = $"coif{taps}";
            Length = 6 * taps;

            switch (taps)
            {
                case 1:
                    LoD = new[] { -0.01565572813546454f,
                                  -0.0727326195128539f,
                                   0.38486484686420286f,
                                   0.8525720202122554f,
                                   0.3378976624578092f,
                                  -0.0727326195128539f };
                    break;
                case 2:
                    LoD = new[] { -0.0007205494453645122f,
                                  -0.0018232088707029932f,
                                   0.0056114348193944995f,
                                   0.023680171946334084f,
                                  -0.0594344186464569f,
                                  -0.0764885990783064f,
                                   0.41700518442169254f,
                                   0.8127236354455423f,
                                   0.3861100668211622f,
                                  -0.06737255472196302f,
                                  -0.04146493678175915f,
                                   0.016387336463522112f };
                    break;
                case 3:
                    LoD = new[] { -3.459977283621256e-05f,
                                  -7.098330313814125e-05f,
                                   0.0004662169601128863f,
                                   0.0011175187708906016f,
                                  -0.0025745176887502236f,
                                  -0.00900797613666158f,
                                   0.015880544863615904f,
                                   0.03455502757306163f,
                                  -0.08230192710688598f,
                                  -0.07179982161931202f,
                                   0.42848347637761874f,
                                   0.7937772226256206f,
                                   0.4051769024096169f,
                                  -0.06112339000267287f,
                                  -0.0657719112818555f,
                                   0.023452696141836267f,
                                   0.007782596427325418f,
                                  -0.003793512864491014f };
                    break;
                case 4:
                    LoD = new[] { -1.7849850030882614e-06f,
                                  -3.2596802368833675e-06f,
                                   3.1229875865345646e-05f,
                                   6.233903446100713e-05f,
                                  -0.00025997455248771324f,
                                  -0.0005890207562443383f,
                                   0.0012665619292989445f,
                                   0.003751436157278457f,
                                  -0.00565828668661072f,
                                  -0.015211731527946259f,
                                   0.025082261844864097f,
                                   0.03933442712333749f,
                                  -0.09622044203398798f,
                                  -0.06662747426342504f,
                                   0.4343860564914685f,
                                   0.782238930920499f,
                                   0.41530840703043026f,
                                  -0.05607731331675481f,
                                  -0.08126669968087875f,
                                   0.026682300156053072f,
                                   0.016068943964776348f,
                                  -0.0073461663276420935f,
                                  -0.0016294920126017326f,
                                   0.0008923136685823146f };
                    break;
                case 5:
                    LoD = new[] { -9.517657273819165e-08f,
                                  -1.6744288576823017e-07f,
                                   2.0637618513646814e-06f,
                                   3.7346551751414047e-06f,
                                  -2.1315026809955787e-05f,
                                  -4.134043227251251e-05f,
                                   0.00014054114970203437f,
                                   0.00030225958181306315f,
                                  -0.0006381313430451114f,
                                  -0.0016628637020130838f,
                                   0.0024333732126576722f,
                                   0.006764185448053083f,
                                  -0.009164231162481846f,
                                  -0.01976177894257264f,
                                   0.03268357426711183f,
                                   0.0412892087501817f,
                                  -0.10557420870333893f,
                                  -0.06203596396290357f,
                                   0.4379916261718371f,
                                   0.7742896036529562f,
                                   0.4215662066908515f,
                                  -0.05204316317624377f,
                                  -0.09192001055969624f,
                                   0.02816802897093635f,
                                   0.023408156785839195f,
                                  -0.010131117519849788f,
                                  -0.004159358781386048f,
                                   0.0021782363581090178f,
                                   0.00035858968789573785f,
                                  -0.00021208083980379827f };
                    break;
                default:
                    throw new ArgumentException("Only coif1-coif5 are supported");
            }
        }

        #endregion
    }
}
