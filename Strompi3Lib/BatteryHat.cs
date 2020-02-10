using System;

namespace Strompi3Lib
{
    public class BatteryHat
    {
        public EBatteryLevel Level { get; private set; }
        public bool IsCharging { get; private set; }
        public EShutdownLevel ShutdownLevel { get; private set; }

        public void SetBatteryState(int sp3BatLevelShutdown, string sp3BatLevel, string sp3Charging)
        {
            try
            {
                IsCharging = Convert.ToInt32(sp3Charging) > 0;

                Level = EBatteryLevel.nothing;
                int level = Convert.ToInt32(sp3BatLevel);
                if (level >= 0 && level <= 4) Level = (EBatteryLevel)level;

                ShutdownLevel = EShutdownLevel.nothing;
                var levelShutdown = sp3BatLevelShutdown;

                if (levelShutdown >= 0 && levelShutdown <= 4) ShutdownLevel = (EShutdownLevel)levelShutdown;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        
        public void SetBatteryShutdownLevel(int sp3BatLevelShutdown)
        {
            try
            {
                ShutdownLevel = EShutdownLevel.nothing;
                var levelShutdown = sp3BatLevelShutdown;

                if (levelShutdown >= 0 && levelShutdown <= 4) ShutdownLevel = (EShutdownLevel)levelShutdown;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
