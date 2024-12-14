﻿namespace InfiniteDecimal;

public partial class BigDec
{
    public static readonly BigDec E = BigDec.Parse(
        "2.7182818284590452353602874713526624977572470936999595749669676277240766303535475945713821785251664274274663919320030599218174135966290435729003342952605956307381323286279434907632338298807531952510190115738341879307021540891499348841675092447614606680822648001684774118537423454424371075390777449920695517027618386062613313845830007520449338265602976067371132007093287091274437470472306969772093101416928368190255151086574637721112523897844250569536967707854499699679468644549059879316368892300987931277361782154249992295763514822082698951936680331825288693984964651058209392398294887933203625094431173012381970684161403970198376793206832823764648042953118023287825098194558153017567173613320698112509961818815930416903515988885193458072738667385894228792284998920868058257492796104841984443634632449684875602336248270419786232090021609902353043699418491463140934317381436405462531520961836908887070167683964243781405927145635490613031072085103837505101157477041718986106873969655212671546889570350354"
    );

    #region Calculated

    public static readonly BigDec E_Sqrt = BigDec.Parse(
        "1.6487212707001281468486507878141635716537761007101480115750793116406610211942156086327765200563666430028666377563077970046711669752196091598409714524900597969294226590984039147199484646594892448968689053364184657208410666568598000889249812117122873752149721955119716090340911156197998698399606426550917545746263044830751947582587826254399319557126900765453228814761009577397884861814432652082034241701047183385915106301256614755338082520260614009728919590840501489150294406956331137767638009584808932951224722635565426541717575241083586972765926066153997676676027916153344711082882095269625790404935685459378957007658732842540903791050754272043732522203670248483545302322846472246269486156013996284571554935118237879959533938396305189301436634709739707453949925599991393256002388517759342648970032996606552334173170721502641632315389155420991972235311866076364177391093171805975842374347615015504601333837900724991125402049382977083625674074150669123348484590253105421863461292455168324381232331276234"
    );

    public static readonly BigDec E_Root4 = BigDec.Parse(
        "1.2840254166877414840734205680624364583362808652814630892175072968722077658672380027533064194395535689016628317496796873058547542360464884275017798987295923100456993054879544143902856284789396238267767771229952277520266322466306470817330699689944830089137387886653407923070193644755769008632493973260896734267022280305488952281848337640193731911188466878182995610252695213374482027360261186306457729632681003017437820874250859254589345784876231507623623526067322107034709397974702977901786944203395857528200121442495575299998177573145664595383602837869600807759973045154879544631376626989511457653114807341446572598150377180989709937601733738721533771855227088016954507503916307175341208909902429726136010183691771779269851972879318121746700551528064874798200449550336789423175570483692754648776914129166658028200221118268085398394266537267203365920984881891051458854629177739629251145316639580721351302906083688615481467960993479987666601111227473892310619415632317672432423354959604727893228474437494"
    );

    public static readonly BigDec E_Root8 = BigDec.Parse(
        "1.1331484530668263168290072278117938725655031317451816259128200360788235778800483865139399907949417285732315270156473075657048210452584733998785564025916995261162759280700397984729320345630340659469435372721057879969170503978449002226363242412105078741499073849703759439478074421417242708873604803092321326823760303608841682868103254484801895448781399602905234105650824927303319551625961103399305295784124165117781794116730287431433627404830979710972141338320524520649639001609330284795588333296588150858774878290361541783709608635695582798165167132536162403592398808868591141143699972192673938561168207463349894590613932515274957328253732128814473748876516692411810121604462108900043717927396474845338606420257719393459575788542690867407226634625420763331925898113825017023795645443195232128179747374888500877214024464864695500348153739252291788508592362159487529574943191057541508242705494367559310534104786110747288650474408470566153246930348082299182110101892189711586691231384631300036528301198499"
    );

    public static readonly BigDec E_Root16 = BigDec.Parse(
        "1.0644944589178594295633905946428896731007254436493533015193075106355639368281660063342934355068766243755129829812611820738543566538970812553245757209431883904365165792021854347144407151321346053960582320674747936929584748216449362626737678486240636654565938873387000030159373894342067468438386167290648040881402991387495630884059068087701479790466562950770735744923650009785796855419414896907106172211390263483191048421324602534853583897343366607352919962768465846295304914201320356380220870550272204630168775127115211212300334859082893161833414331020142892699670493842161237338322898871978430252467999408996161174875131729855851318915451399877891624423359854494842393836011749092823351954365964822658043494079147855068647675074250556564065142135216031953746314626771193575929473591814576839561081745164730990045857547024798588523120184519075941041693984868753909304895708095495354427561790209823496452377959729324027958117760506086600667940234075248468111422612273545340667205307101364569098067070464"
    );

    public static readonly BigDec E_Root32 = BigDec.Parse(
        "1.0317434074991026709387478152815071441944983266418160960083487433214163336126074984792558934650215421613611856135991619471875090553379212773641077457382322259121368995365922221945697841360308466333986258940924154285788904944587546180441885980684544524411805878949852729247784424337641541816350460618425115523971645510968789391281439309424256296760729999496642159616376841968935659681196064053586859454322456598155847422932462386774964372857705249635672667290055378115597944852337839442879180601084009327048870684950613704757287850081168618403284932979738459187120288373442900195843298371885584557499673180401816898196609506443836434849108929698435060747829450083372334551819784848314502044890000156877773605149963381068867956832732051675175764598477886979042988415190075702095299510305052833768036364902357387469821471491992585219211098202337610229402441884238245856417134959226670918232219959810271330013410984173447243155303815154714653190702604629187829202070753027976395240141839757575641825274143"
    );

    #endregion

    public static readonly BigDec PI = BigDec.Parse(
        "3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679821480865132823066470938446095505822317253594081284811174502841027019385211055596446229489549303819644288109756659334461284756482337867831652712019091456485669234603486104543266482133936072602491412737245870066063155881748815209209628292540917153643678925903600113305305488204665213841469519415116094330572703657595919530921861173819326117931051185480744623799627495673518857527248912279381830119491298336733624406566430860213949463952247371907021798609437027705392171762931767523846748184676694051320005681271452635608277857713427577896091736371787214684409012249534301465495853710507922796892589235420199561121290219608640344181598136297747713099605187072113499999983729780499510597317328160963185950244594553469083026425223082533446850352619311881710100031378387528865875332083814206171776691473035982534904287554687311595628638823537875937519577818577805321712268066130019278766111959092164201989"
    );

    /// <summary>
    /// Max "Decimal" scale
    /// </summary>
    /// <url>https://learn.microsoft.com/en-us/dotnet/api/system.decimal.scale?view=net-9.0</url>
    public const int MaxDecimalScale = 28;

    public static readonly BigDec MaxDecimalValue;
    public static readonly BigDec MinAbsDecimalValue;
}