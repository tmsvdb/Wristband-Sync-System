
using System.Collections;
using System.Collections.Generic;
using System;

namespace Wristband 
{
    public class UserData 
    {
        /*
        ====================================
        PLAYER DATA REFERENCES:
        ====================================
        */
        public string DateOfBirth()
        {
            throw new NotImplementedException();
            //return AtlasServicesManager.Instance.users.userData.date_of_birth.Split('T')[0];
        }

        public string Gender () { 
            throw new NotImplementedException();
            //return AtlasServicesManager.Instance.users.userData.gender; 
        }

        public float Weight () { 
            throw new NotImplementedException();
            //return AtlasServicesManager.Instance.boosth.bodyData.weight; 
        }

        public float Height () { 
            throw new NotImplementedException();
            //return AtlasServicesManager.Instance.boosth.bodyData.height; 
        }

        public int DistancePerStep() {
            throw new NotImplementedException();
            //return AtlasServicesManager.Instance.boosth.bodyData.distance_per_step;
        }

        public int StepsTarget() { 
            throw new NotImplementedException();
            //return AtlasServicesManager.Instance.boosth.bodyData.steps_target; 
        }
    }
}