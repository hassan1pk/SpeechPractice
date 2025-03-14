using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;

namespace SpeechPractice
{
    public static class TongueTwister
    {
        public static void GenerateAudio()
        {
            string inputFile = "tonguetwister.txt"; // Input text file
            string tempWavFile = "tonguetwister.wav"; // Temporary WAV file
            string finalMp3File = "tonguetwister.mp3"; // Final MP3 output file

            if (!File.Exists(inputFile))
            {
                Console.WriteLine("Input file not found.");
                return;
            }

            //using (var writer = new WaveFileWriter("temp.wav", new WaveFormat(44100, 16, 1)))
            //{

            //        Console.WriteLine($"Creating test wav");


            //    // Insert silence (1 sec)
            //    byte[] silence = new byte[44100 * 4]; // 1 sec silence at 16kHz
            //    writer.Write(silence, 0, silence.Length);


            //}

            // Generate a single WAV file with all spoken text
            GenerateSpeechWav(inputFile, tempWavFile);

            // Convert WAV to MP3 with the correct sample rate
            ConvertWavToMp3(tempWavFile, finalMp3File);

            // Clean up
            File.Delete(tempWavFile);

            Console.WriteLine("MP3 file has been created successfully!");
        }

        static void GenerateSpeechWav(string inputFile, string outputWavFile)
        {
            using (var synthesizer = new SpeechSynthesizer())
            {
                synthesizer.SetOutputToWaveFile(outputWavFile);
                synthesizer.Rate = 0; // Ensure normal speech rate 

                var installedVoices = synthesizer.GetInstalledVoices();
                Console.WriteLine($"{installedVoices.Count} voices found");


                // Force correct audio format (16-bit, 16kHz, mono)
                synthesizer.SetOutputToAudioStream(
                    new MemoryStream(),
                    new SpeechAudioFormatInfo(24000, AudioBitsPerSample.Sixteen, AudioChannel.Mono)
                );

                using (var writer = new WaveFileWriter(outputWavFile, new WaveFormat(24000, 16, 1)))
                {
                    foreach (var line in File.ReadLines(inputFile))
                    {
                        Console.WriteLine($"Processing: {line}");


                        for (int i = 0; i < 5; i++) // Repeat each line 3 times
                        {
                            synthesizer.SelectVoice(installedVoices[i % installedVoices.Count].VoiceInfo.Name);
                            if (i == 0)
                            {
                                synthesizer.Rate = -5;
                            }
                            else if (i == 1)
                            {
                                synthesizer.Rate = -4;
                            }
                            else if (i == 2)
                            {
                                synthesizer.Rate = -3;
                            }
                            else if (i == 3)
                            {
                                synthesizer.Rate = -2;
                            }
                            else if (i == 4)
                            {
                                synthesizer.Rate = -1;
                            }
                            else
                            {
                                synthesizer.Rate = 0;
                            }

                            byte[] speechAudio = SpeakToBytes(synthesizer, line);
                            writer.Write(speechAudio, 0, speechAudio.Length);

                            // Insert silence (1 sec)
                            //byte[] silence = new byte[24000 * 2]; // 1 sec silence at 16kHz
                            byte[] silence = new byte[speechAudio.Length + speechAudio.Length];
                            writer.Write(silence, 0, silence.Length);
                        }
                    }
                }
            }
        }

        static byte[] SpeakToBytes(SpeechSynthesizer synthesizer, string text)
        {
            using (var memoryStream = new MemoryStream())
            {
                synthesizer.SetOutputToWaveStream(memoryStream);
                synthesizer.Speak(text);
                return memoryStream.ToArray();
            }
        }

        static void ConvertWavToMp3(string wavFile, string mp3File)
        {
            using (var reader = new WaveFileReader(wavFile))
            using (var resampler = new MediaFoundationResampler(reader, new WaveFormat(16000, 1)))
            {
                resampler.ResamplerQuality = 60; // High quality
                MediaFoundationEncoder.EncodeToMp3(resampler, mp3File);
            }
        }
    }
}
