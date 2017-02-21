makecert -n "CN=TempCA" -r -sv TempCA.pvk TempCA.cer
CertMgr.exe /add TempCA.cer /s /r localMachine root
makecert -sr CurrentUser -ss MY -a sha1 -n "CN=WCFHOLClient" -sky exchange -pe -iv TempCA.pvk -ic TempCA.cer
makecert -sr LocalMachine -ss MY -a sha1 -n "CN=WCFHOLService" -sky exchange -pe -iv TempCA.pvk -ic TempCA.cer
makecert -sr LocalMachine -ss MY -a sha1 -n "CN=WCFHOLSTS" -sky exchange -pe -iv TempCA.pvk -ic TempCA.cer
