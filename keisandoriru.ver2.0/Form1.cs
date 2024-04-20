using keisandoriru.ver2._0.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*TODO
 * 関数を綺麗に並び変える
 */

namespace keisandoriru.ver2._0 {
    public partial class Form1:Form {
        Random rand = new Random();//乱数
        Boolean pause = true;//ゲームの一時停止と再開用変数

        Boolean[] q = new Boolean[4];//出題する演算方法の設定
        int[] calc_score_w = { 1,2,3,3 };//演算方法によるスコアの重みを決定.
        int qs = -1;//配列qのどのインデックスが選択されたか格納するための変数
        /*
         * 0:足し算
         * 1:引き算
         * 2:掛け算
         * 3:割り算
         */

        int difficulty = 1;
        int[] difficulty_score_w = { 1,2,3 };//難易度によるスコアの重みを決定
        /*
         * 1:やさしい
         * 2:ふつう
         * 3:むずかしい
         */

        int x = 0;
        int y = 0;

        int score = 0;//スコア

        //累計
        int correct_c = 0;//正解数
        int incorrect_c = 0;//不正解数
        double correct_p = 0;//正解率
        //30問
        int[] correct_c_30 = new int[30];
        int correct_c_30_sum = 0;
        double correct_p_30 = 0;
        //50問
        int[] correct_c_50 = new int[50];
        int correct_c_50_sum = 0;
        double correct_p_50 = 0;

        //称号管理用二次元配列
        Bitmap[,] achievement = {
            {Properties.Resources._1_100,Properties.Resources._1_100f},
            {Properties.Resources._1_50,Properties.Resources._1_50f},
            {Properties.Resources._1_30,Properties.Resources._1_30f},
            {Properties.Resources._2_100,Properties.Resources._2_100f},
            {Properties.Resources._2_50,Properties.Resources._2_50f},
            {Properties.Resources._2_30,Properties.Resources._2_30f},
            {Properties.Resources._3_100,Properties.Resources._3_100f},
            {Properties.Resources._3_50,Properties.Resources._3_50f},
            {Properties.Resources._3_30,Properties.Resources._3_30f},
            {Properties.Resources.master,Properties.Resources.masterf},
        };
        int[] achievement_ = { 1,1,1,1,1,1,1,1,1,1 };//0だったら達成1だったら未達成
        //int[] achievement_ = { 0,1,0,0,0,0,0,0,0,1 };//0だったら達成1だったら未達成
        PictureBox[] pic = new PictureBox[10];

        /*
         * やる気があったらやる
         * 0だったら未達成1だったら達成
         */

        public Form1() {
            InitializeComponent();
        }

        //ロードされた時の処理
        private void Form1_Load(object sender,EventArgs e) {
            //称号画像の初期化
            load_pic();
            load_achievement();
            reset();
            label16.Text="スコア : "+score;//スコアの初期化
            radioButton1.Checked=true;//難易度の初期値
            trackBar1.Minimum=1;//トラックバー初期値設定
            trackBar1.Maximum=30;
            trackBar1.Value=15;
            trackBar1.LargeChange=5;
            label15.Text="回答時間 : "+trackBar1.Value+"秒";
            progressBar1.Maximum=trackBar1.Value;//問題切り替え時間の設定
            //出題演算の設定初期化
            checkBox1.Checked=true;
            int i = 3;
            foreach(CheckBox cb in groupBox4.Controls) {
                if(cb.Checked) {
                    q[i]=true;
                }
                else {
                    q[i]=false;
                }
                i--;
            }
            //ここまで
        }





        //タイマー系
        //タイマーon,offの切り替え処理
        private void button1_Click(object sender,EventArgs e) {
            if(pause) {
                pause=false;
                button1.Text="すとっぷ";
                timer1.Enabled=true;
                timer1.Interval=1000;
            }
            else {
                pause=true;
                button1.Text="すたーと";
                timer1.Enabled=false;
            }
        }
        private void timer1_Tick(object sender,EventArgs e) {
            if(progressBar1.Value==0) {//問題作成
                textBox1.Text="";
                check_achievement();
                do {//演算方法の決定
                    qs=rand.Next(0,q.Length);
                } while(!q[qs]);
                q_create();
            }
            if(progressBar1.Value==progressBar1.Maximum) {//リセット(不正解判定)
                incorrect_count_add();
                progressBar1.Value=0;
            }
            else {
                progressBar1.Value+=1;
            }
        }





        //設定変更系
        //出題演算の設定更新処理
        private void checkBox1_CheckedChanged(object sender,EventArgs e) {//出題する計算の変更
            int i = 3;
            int j = 0;
            foreach(CheckBox cb in groupBox4.Controls) {
                if(cb.Checked) {
                    q[i]=true;
                }
                else {
                    q[i]=false;
                }
                i--;
            }
            foreach(Boolean b in q) {
                if(b==true) {
                    break;
                }
                else {
                    j++;
                }
                if(j==4) {
                    MessageBox.Show("全て選択されていないので選択状況を初期化しました.","エラー");
                    checkBox1.Checked=true;
                    checkBox2.Checked=false;
                    checkBox3.Checked=false;
                    checkBox4.Checked=false;
                }
            }
        }
        //回答時間の設定更新処理
        private void trackBar1_Scroll(object sender,EventArgs e) {
            progressBar1.Maximum=trackBar1.Value;
            label15.Text="回答時間 : "+trackBar1.Value+"秒";
        }
        //難易度の設定更新処理
        private void radioButton1_CheckedChanged(object sender,EventArgs e) {
            int i = 3;
            foreach(RadioButton rb in groupBox6.Controls) {
                if(rb.Checked) {
                    difficulty=i;
                    reset();
                    break;
                }
                i--;
            }
        }





        //問題作成処理
        private void q_create() {
            string result = "";//x ? yの文字列を作成
            switch(qs) {
                case 0://足し算
                    q_create_plus();
                    result=x+" + "+y;
                    break;
                case 1://引き算
                    q_create_minus();
                    result=x+" - "+y;
                    break;
                case 2://掛け算
                    q_create_multi();
                    result=x+" × "+y;
                    break;
                case 3://割り算
                    q_create_divid();
                    result=x+" ÷ "+y;
                    break;
            }
            label1.Text=result;
        }
        //難易度による問題作成処理
        /*
         * case 1 やさしい
         * case 2 ふつう
         * case 3 むずかしい
         */
        void q_create_plus() {//足し算
            switch(difficulty) {
                case 1:
                    x=rand.Next(1,10);
                    y=rand.Next(1,10);
                    break;
                case 2:
                    x=rand.Next(11,100);
                    y=rand.Next(11,100);
                    break;
                case 3:
                    x=rand.Next(100,1000);
                    y=rand.Next(100,1000);
                    break;
            }
        }
        void q_create_minus() {//引き算
            do {
                switch(difficulty) {
                    case 1:
                        x=rand.Next(1,10);
                        y=rand.Next(1,10);
                        break;
                    case 2:
                        x=rand.Next(11,100);
                        y=rand.Next(11,100);
                        break;
                    case 3:
                        x=rand.Next(100,1000);
                        y=rand.Next(100,1000);
                        break;
                }
            } while((x-y)<0);
        }
        void q_create_multi() {//掛け算
            switch(difficulty) {
                case 1:
                    x=rand.Next(1,10);
                    y=rand.Next(1,10);
                    break;
                case 2:
                    x=rand.Next(11,50);
                    y=rand.Next(1,10);
                    break;
                case 3:
                    x=rand.Next(50,100);
                    y=rand.Next(1,10);
                    break;
            }
        }
        void q_create_divid() {//割り算
            do {
                switch(difficulty) {
                    case 1:
                        x=rand.Next(1,10);
                        y=rand.Next(2,10);
                        break;
                    case 2:
                        x=rand.Next(100);
                        y=rand.Next(2,10);
                        break;
                    case 3:
                        x=rand.Next(1000);
                        y=rand.Next(2,10);
                        break;
                }
            } while((x%y)!=0);
        }





        //回答系
        //回答処理
        private void textBox1_KeyDown(object sender,KeyEventArgs e) {
            try {
                if(e.KeyCode==Keys.Enter) {
                    answer();
                }
            }
            catch {//未入力で入力または文字で入力した時の例外処理

            }
        }
        //回答が正解しているか判断する処理
        private void answer() {
            Boolean judge = false;
            switch(qs) {
                case 0:
                    if((x+y)==int.Parse(textBox1.Text)) {
                        judge=true;
                    }
                    break;
                case 1:
                    if((x-y)==int.Parse(textBox1.Text)) {
                        judge=true;
                    }
                    break;
                case 2:
                    if((x*y)==int.Parse(textBox1.Text)) {
                        judge=true;
                    }
                    break;
                case 3:
                    if((x/y)==int.Parse(textBox1.Text)) {
                        judge=true;
                    }
                    break;
            }
            if(judge) {
                score_add();
                correct_count_add();
                label1.Text="正解";
            }
            else {
                textBox1.Text="";
            }
        }





        //スコア系
        //スコア加算処理
        private void score_add() {
            score+=calc_score_w[qs]*difficulty_score_w[difficulty-1]+((trackBar1.Maximum-trackBar1.Value)/5);//演算方法*難易度+(回答時間最大値-回答時間)/5
            label16.Text="スコア : "+score;
            progressBar1.Value=0;
        }
        //正解数,正解率計算処理
        //正解数を追加
        private void correct_count_add() {
            correct_c++;
            correct_c_30=array_lshift(correct_c_30);
            correct_c_30[29]=1;
            correct_c_50=array_lshift(correct_c_50);
            correct_c_50[49]=1;
            correct_c_30_sum=array_count(correct_c_30);
            correct_c_50_sum=array_count(correct_c_50);
            correct_p_calc();
        }
        //不正解数を追加
        private void incorrect_count_add() {
            incorrect_c++;
            correct_c_30=array_lshift(correct_c_30);
            correct_c_50=array_lshift(correct_c_50);
            correct_c_30_sum=array_count(correct_c_30);
            correct_c_50_sum=array_count(correct_c_50);
            correct_p_calc();
        }
        //正解率の計算
        private void correct_p_calc() {
            correct_p=(double)correct_c/(correct_c+incorrect_c);
            correct_p_30=(double)correct_c_30_sum/30;
            correct_p_50=(double)correct_c_50_sum/50;
            correct_score_change();
        }
        //表示変更
        private void correct_score_change() {
            label8.Text=correct_c.ToString();
            label9.Text=Math.Round((correct_p*100),2).ToString()+"%";
            label10.Text=correct_c_30_sum.ToString();
            label11.Text=Math.Round((correct_p_30*100),2).ToString()+"%";
            label12.Text=correct_c_50_sum.ToString();
            label13.Text=Math.Round((correct_p_50*100),2).ToString()+"%";
        }




        //配列関係
        //配列の中身を1つずらす処理
        private int[] array_lshift(int[] a) {
            int[] result = new int[a.Length];
            for(int i = 0;i<a.Length-1;i++) {//a.length-1にしてるのはa[i+1]してるから配列の範囲を超えてしまうため
                result[i]=a[i+1];
            }
            result[a.Length-1]=0;
            return result;
        }
        //配列の中身をすべて合計
        private int array_count(int[] a) {
            int result = 0;
            for(int i = 0;i<a.Length;i++) {
                result+=a[i];
            }
            return result;
        }





        //称号関係
        //ロード
        private void load_pic() {
            pic[0]=pictureBox1; 
            pic[1]=pictureBox2; 
            pic[2]=pictureBox3; 
            pic[3]=pictureBox4; 
            pic[4]=pictureBox5; 
            pic[5]=pictureBox6; 
            pic[6]=pictureBox7; 
            pic[7]=pictureBox8; 
            pic[8]=pictureBox9; 
            pic[9]=pictureBox10; 
        }
        private void load_achievement() {//称号画像をすべて未達成に変える
            for(int i = 0;i<pic.Length;i++) {
                if(achievement_[i] == 1) {
                    pic[i].Image = achievement[i,1];
                }else {
                    pic[i].Image = achievement[i,0];
                }
            }
        }
        private void check_achievement() {//称号の取得条件を達成しているか確認
            if(correct_c >= 100 && correct_p >= 0.7 ) {//正解数100超えて正解率が70%以上だった時
                switch(difficulty) {
                    case 1:
                        achievement_[0]=0;
                        break;
                    case 2:
                        achievement_[3]=0;
                        break;
                    case 3:
                        achievement_[6]=0;
                        break;
                }
            }
            if(correct_c_50_sum == 50 && correct_p_50 ==1) {//50問中正解率が100%だった時
                switch(difficulty) {
                    case 1:
                        achievement_[1]=0;
                        break;
                    case 2:
                        achievement_[4]=0;
                        break;
                    case 3:
                        achievement_[7]=0;
                        break;
                }
            }
            if(correct_c_30_sum==30&&correct_p_30==1) {//30問中正解率が100%だった時
                switch(difficulty) {
                    case 1:
                        achievement_[2]=0;
                        break;
                    case 2:
                        achievement_[5]=0;
                        break;
                    case 3:
                        achievement_[8]=0;
                        break;
                }

            }
            for(int i = 0;i < achievement_.Length-1;i++) {//すべての称号を取っているか
                if(achievement_[i]!=0) {
                    break;
                }else if(i == achievement_.Length-2) {
                    achievement_[9]=0;
                }
            }
            load_achievement();
        }





        //データの保存
        private void button3_Click(object sender,EventArgs e) {
            string path = "data.csv";
            using (var sw = new StreamWriter(path)) {
                for(int i = 0;i<achievement_.Length;i++) {
                    if(i ==achievement_.Length-1) {
                        sw.Write(achievement_[i]);
                    }else {
                        sw.Write(achievement_[i]+",");
                    }
                }
                sw.Close();
                label17.Text="データを保存しました.";
            }
        }
        //データの読み込み
        private void button2_Click(object sender,EventArgs e) {
            string path = "data.csv";
            string[] temp = new string[achievement_.Length];
            if(File.Exists(path) == false) {
                MessageBox.Show("ファイルが見つかりません.");
            }else {
                using(var sr = new StreamReader(path)) {
                    string line = sr.ReadLine();
                    sr.Close();
                    temp=line.Split(',');
                }
                for(int i = 0;i<temp.Length;i++) {
                    achievement_[i] =int.Parse(temp[i]);
                    load_achievement();
                }
                label17.Text="データを読み込みました.";
            }
        }





        //リセット
        private void reset() {
            timer1.Enabled = false;
            button1.Text="すたーと";
            label1.Text="〇 + 〇";
            correct_c=0;
            incorrect_c = 1;
            progressBar1.Value=0;
            //配列初期化
            for(int j = 0;j<correct_c_30.Length;j++) {
                correct_c_30[j]=0;
            }
            for(int j = 0;j<correct_c_50.Length;j++) {
                correct_c_50[j]=0;
            }
            correct_c_30_sum=array_count(correct_c_30);
            correct_c_50_sum=array_count(correct_c_50);
            correct_p_calc();
        }

        private void button4_Click(object sender,EventArgs e) {
            reset();
            score=0;
            label16.Text="スコア : "+score;
            MessageBox.Show("リセットしました.");
        }
    }
}
