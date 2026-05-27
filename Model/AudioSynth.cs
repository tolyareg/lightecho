using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace LightEcho.Model
{
    public static class AudioSynth
    {
        private static byte[] _selectWav;
        private static byte[] _chargeWav;
        private static byte[] _burstWav;
        private static byte[] _repairWav;
        private static byte[] _killWav;
        private static byte[] _victoryWav;
        private static byte[] _gameOverWav;
        private static byte[] _leechDrainWav;
        private static byte[] _bgmWav;
        private static MemoryStream _bgmStream;
        private static SoundPlayer _bgmPlayer;

        static AudioSynth()
        {
            try
            {
                _selectWav = CreateSelectSound();
                _chargeWav = CreateChargeSound();
                _burstWav = CreateBurstSound();
                _repairWav = CreateRepairSound();
                _killWav = CreateKillSound();
                _victoryWav = CreateVictorySound();
                _gameOverWav = CreateGameOverSound();
                _leechDrainWav = CreateLeechDrainSound();
                _bgmWav = CreateBackgroundMusic();
            }
            catch
            {
            }
        }

        public static void StartBGM()
        {
            Task.Run(() =>
            {
                try
                {
                    if (_bgmWav == null)
                    {
                        _bgmWav = CreateBackgroundMusic();
                    }
                    if (_bgmStream == null)
                    {
                        _bgmStream = new MemoryStream(_bgmWav);
                    }
                    if (_bgmPlayer == null)
                    {
                        _bgmPlayer = new SoundPlayer(_bgmStream);
                    }
                    _bgmPlayer.PlayLooping();
                }
                catch
                {
                }
            });
        }

        public static void StopBGM()
        {
            try
            {
                _bgmPlayer?.Stop();
            }
            catch {}
        }

        public static void PlaySelect() => PlayWav(_selectWav);
        public static void PlayCharge() => PlayWav(_chargeWav);
        public static void PlayBurst() => PlayWav(_burstWav);
        public static void PlayRepair() => PlayWav(_repairWav);
        public static void PlayKill() => PlayWav(_killWav);
        public static void PlayVictory() => PlayWav(_victoryWav);
        public static void PlayGameOver() => PlayWav(_gameOverWav);
        public static void PlayLeechDrain() => PlayWav(_leechDrainWav);

        private static void PlayWav(byte[] wavBytes)
        {
            if (wavBytes == null) return;
            Task.Run(() =>
            {
                try
                {
                    using (var ms = new MemoryStream(wavBytes))
                    {
                        using (var player = new SoundPlayer(ms))
                        {
                            player.PlaySync();
                        }
                    }
                }
                catch
                {
                }
            });
        }

        private static byte[] CreateSelectSound()
        {
            int sampleRate = 22050;
            double duration = 0.05;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double freq = 1000.0 - (t / duration) * 300.0;
                double env = Math.Exp(-50.0 * t);
                samples[i] = (float)(Math.Sin(2 * Math.PI * freq * t) * env * 0.18);
            }
            return BuildWav(samples, sampleRate);
        }

        private static byte[] CreateChargeSound()
        {
            int sampleRate = 22050;
            double duration = 0.14;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double freq = 329.63 + (t / duration) * 110.37;
                double env = Math.Sin(Math.PI * (t / duration)) * Math.Exp(-2.5 * t);
                samples[i] = (float)(Math.Sin(2 * Math.PI * freq * t) * env * 0.12);
            }
            return BuildWav(samples, sampleRate);
        }

        private static byte[] CreateBurstSound()
        {
            int sampleRate = 22050;
            double duration = 0.55;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            
            double[] notes = { 783.99, 1046.50, 1318.51 };
            
            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double sum = 0;
                
                for (int n = 0; n < notes.Length; n++)
                {
                    double delay = n * 0.07;
                    if (t >= delay)
                    {
                        double localT = t - delay;
                        double freq = notes[n];
                        double p = freq * (1.0 + 0.04 * Math.Exp(-20.0 * localT));
                        double env = Math.Exp(-7.5 * localT);
                        sum += Math.Sin(2 * Math.PI * p * localT) * env * 0.15;
                    }
                }
                
                double masterEnv = Math.Min(1.0, (duration - t) / 0.1);
                samples[i] = (float)(sum * masterEnv);
            }
            return BuildWav(samples, sampleRate);
        }

        private static byte[] CreateRepairSound()
        {
            int sampleRate = 22050;
            double duration = 0.07;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double freq = 520.0 - (t / duration) * 100.0;
                double env = Math.Sin(Math.PI * (t / duration)) * Math.Exp(-8.0 * t);
                samples[i] = (float)(Math.Sin(2 * Math.PI * freq * t) * env * 0.10);
            }
            return BuildWav(samples, sampleRate);
        }

        private static byte[] CreateKillSound()
        {
            int sampleRate = 22050;
            double duration = 0.22;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            Random r = new Random();
            
            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double freq = 1700.0 * Math.Exp(-14.0 * t);
                double sine = Math.Sin(2 * Math.PI * freq * t);
                
                double fizz = (r.NextDouble() * 2.0 - 1.0) * Math.Exp(-30.0 * t);
                
                double env = Math.Exp(-9.0 * t);
                samples[i] = (float)((sine * 0.88 + fizz * 0.12) * env * 0.22);
            }
            return BuildWav(samples, sampleRate);
        }

        private static byte[] CreateLeechDrainSound()
        {
            int sampleRate = 22050;
            double duration = 0.42;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double f1 = 250.0 * Math.Exp(-3.5 * t) + 110.0;
                double f2 = f1 * 1.06;
                
                double wave = Math.Sin(2 * Math.PI * f1 * t) * 0.55 + Math.Sin(2 * Math.PI * f2 * t) * 0.45;
                double vibrato = 1.0 + 0.12 * Math.Sin(2 * Math.PI * 15.0 * t);
                
                double env = Math.Sin(Math.PI * (t / duration)) * Math.Exp(-1.8 * t);
                samples[i] = (float)(wave * vibrato * env * 0.24);
            }
            return BuildWav(samples, sampleRate);
        }

        private static byte[] CreateVictorySound()
        {
            int sampleRate = 22050;
            double duration = 2.2;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];
            
            double[] arpeggio = { 261.63, 329.63, 392.00, 523.25, 659.25, 783.99, 1046.50 };
            
            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double val = 0;
                
                for (int n = 0; n < arpeggio.Length; n++)
                {
                    double delay = n * 0.11;
                    if (t >= delay)
                    {
                        double localT = t - delay;
                        double freq = arpeggio[n];
                        double env = Math.Exp(-2.0 * localT);
                        double sine = Math.Sin(2 * Math.PI * freq * localT);
                        double tri = Math.Abs((localT * freq) % 1.0 - 0.5) * 4.0 - 1.0;
                        double mixedRaw = sine * 0.85 + tri * 0.15;
                        
                        val += mixedRaw * env * 0.11;
                    }
                }
                
                if (t >= 0.25)
                {
                    double localT = t - 0.25;
                    double env = Math.Exp(-0.9 * localT);
                    val += Math.Sin(2 * Math.PI * 130.81 * localT) * env * 0.10;
                }
                
                double masterEnvelope = Math.Min(1.0, (duration - t) / 0.4);
                samples[i] = (float)(val * masterEnvelope);
            }
            return BuildWav(samples, sampleRate);
        }

        private static byte[] CreateGameOverSound()
        {
            int sampleRate = 22050;
            double duration = 2.0;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            double[] lowSadChord = { 130.81, 155.56, 196.00 };
            
            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double val = 0;
                
                for (int n = 0; n < lowSadChord.Length; n++)
                {
                    double freq = lowSadChord[n];
                    double slidingFreq = freq * Math.Exp(-0.4 * t);
                    double sine = Math.Sin(2 * Math.PI * slidingFreq * t);
                    
                    double env = Math.Exp(-1.1 * t);
                    val += sine * env * 0.16;
                }
                
                double masterEnvelope = Math.Min(1.0, (duration - t) / 0.35);
                samples[i] = (float)(val * masterEnvelope);
            }
            return BuildWav(samples, sampleRate);
        }

        private static byte[] CreateBackgroundMusic()
        {
            int sampleRate = 22050;
            double duration = 8.0;
            int numSamples = (int)(sampleRate * duration);
            float[] samples = new float[numSamples];

            double[][] chords = new double[][]
            {
                new double[] { 130.81, 196.00, 261.63, 329.63 }, // C
                new double[] { 196.00, 293.66, 392.00, 493.88 }, // G
                new double[] { 110.00, 164.81, 220.00, 261.63 }, // Am
                new double[] { 87.31, 130.81, 174.61, 220.00 }   // F
            };

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                
                int chordIndex = (int)(t / 2.0) % 4;
                double[] chord = chords[chordIndex];
                
                double bassTime = t % 0.5;
                double bassFreq = (t % 1.0 < 0.5) ? chord[0] : chord[1];
                double bassEnv = Math.Max(0.0, 1.0 - (bassTime / 0.44)) * Math.Min(1.0, bassTime / 0.012);
                
                double bassPhase = t * bassFreq;
                double bassWave = Math.Abs((bassPhase % 1.0) - 0.5) * 4.0 - 1.0;
                
                double arpTime = t % 0.125;
                int step = (int)(t / 0.125) % 16;
                
                double arpFreq;
                if (step % 8 == 7) arpFreq = chord[3] * 2.0;
                else if (step % 8 == 6) arpFreq = chord[2] * 2.0; 
                else if (step % 8 == 5) arpFreq = chord[1] * 2.0;
                else arpFreq = chord[step % 4];
                
                double arpPhase = t * arpFreq;
                double arpWave = (arpPhase % 1.0 < 0.25) ? 1.0 : -1.0;
                double arpEnv = Math.Max(0.0, 1.0 - (arpTime / 0.10)) * Math.Min(1.0, arpTime / 0.004);
                
                double padWave1 = Math.Sin(2 * Math.PI * chord[1] * t);
                double padWave2 = Math.Sin(2 * Math.PI * chord[2] * t);
                double padSum = (padWave1 + padWave2) * 0.45;
                
                samples[i] = (float)((bassWave * bassEnv * 0.075) + (arpWave * arpEnv * 0.024) + (padSum * 0.035));
            }

            return BuildWav(samples, sampleRate);
        }

        private static byte[] BuildWav(float[] samples, int sampleRate)
        {
            int numSamples = samples.Length;
            int dataSize = numSamples * 2;
            int fileSize = 36 + dataSize;
            byte[] wav = new byte[44 + dataSize];

            System.Text.Encoding.ASCII.GetBytes("RIFF").CopyTo(wav, 0);
            BitConverter.GetBytes(fileSize).CopyTo(wav, 4);
            System.Text.Encoding.ASCII.GetBytes("WAVE").CopyTo(wav, 8);
            System.Text.Encoding.ASCII.GetBytes("fmt ").CopyTo(wav, 12);
            BitConverter.GetBytes(16).CopyTo(wav, 16);
            BitConverter.GetBytes((short)1).CopyTo(wav, 20); 
            BitConverter.GetBytes((short)1).CopyTo(wav, 22);
            BitConverter.GetBytes(sampleRate).CopyTo(wav, 24);
            BitConverter.GetBytes(sampleRate * 2).CopyTo(wav, 28);
            BitConverter.GetBytes((short)2).CopyTo(wav, 32);
            BitConverter.GetBytes((short)16).CopyTo(wav, 34);
            System.Text.Encoding.ASCII.GetBytes("data").CopyTo(wav, 36);
            BitConverter.GetBytes(dataSize).CopyTo(wav, 40);

            for (int i = 0; i < numSamples; i++)
            {
                short val = (short)(Math.Max(-1f, Math.Min(1f, samples[i])) * 32767f);
                BitConverter.GetBytes(val).CopyTo(wav, 44 + i * 2);
            }
            return wav;
        }
    }
}
