using UnityEngine.Assertions;
using Offworld.GameCore.Text;
using Offworld.SystemCore;

namespace Offworld.GameCore
{
    public static class Globals
    {
        private static PoolManager mPoolManager;
        private static GameFactory mGameFactory;
        private static Infos mInfos;
        private static TextManager mTextManager;
        private static Campaign mCampaign;
        private static INetwork mNetwork;
        private static IAppInfo mAppInfo;

        public static PoolManager PoolManager               { get { return mPoolManager;            } }
        public static GameFactory Factory                   { get { return mGameFactory;            } }
        public static Infos Infos                           { get { return mInfos;                  } }
        public static TextManager TextManager               { get { return mTextManager;            } }
        public static Campaign Campaign                     { get { return mCampaign;               } }
        public static INetwork Network                      { get { return mNetwork;                } }
        public static IAppInfo AppInfo                      { get { return mAppInfo;                } }

        public static void Initialize(IInfosXMLLoader xmlLoader, INetwork network, IAppInfo appInfo)
        {
            using (new UnityProfileScope("Globals.Initialize"))
            {
                Assert.IsNotNull(xmlLoader);
                Assert.IsNotNull(network);
                Assert.IsNotNull(appInfo);
                mPoolManager    = new PoolManager();
                mGameFactory    = new GameFactory();
                mInfos          = new Infos(xmlLoader);
                mTextManager    = new TextManager(mInfos);
                mNetwork        = network;
                mAppInfo        = appInfo;
            }
        }

        public static void SetCampaign(Campaign campaign)
        {
            mCampaign = campaign;
        }

        public static void SetFactory(GameFactory factory)
        {
            Assert.IsNotNull(factory);
            mGameFactory = factory;
        }

        public static void Shutdown()
        {
            mPoolManager    = null;
            mInfos          = null;
            mTextManager    = null;
            mCampaign       = null;
            mNetwork        = null;
        }

        public static void Update()
        {
            if (mPoolManager != null)
                mPoolManager.Update();
        }
    }
}