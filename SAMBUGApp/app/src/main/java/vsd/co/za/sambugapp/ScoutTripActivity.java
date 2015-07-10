package vsd.co.za.sambugapp;

import android.content.Intent;
import android.graphics.BitmapFactory;
import android.graphics.drawable.Drawable;
import android.support.v7.app.ActionBarActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.HorizontalScrollView;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.RelativeLayout;
import android.widget.TextView;

import java.util.ArrayList;


public class ScoutTripActivity extends ActionBarActivity {

    public static final String SCOUT_STOP="za.co.vsd.scout_stop";
    private final String TAG="ScoutTripActivity";
    private ScoutTrip mScoutTrip;
    private ListView mLstStops;
    private Button mBtnAddStop;
    private ListView mLstPestsPerTree;
    private ScoutStopAdapter lstStopsAdapter;
    private PestsPerTreeAdapter lstPestsPerTreeAdapter;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_scout_trip);
        mScoutTrip = new ScoutTrip();
        mLstStops = (ListView) findViewById(R.id.lstStops);
        mLstStops.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                updateStop(position);
            }
        });
        lstStopsAdapter = new ScoutStopAdapter(mScoutTrip.getList());
        mLstStops.setAdapter(lstStopsAdapter);

        mLstPestsPerTree = (ListView) findViewById(R.id.lstPestsPerTree);
        lstPestsPerTreeAdapter = new PestsPerTreeAdapter(mScoutTrip.getList());
        mLstPestsPerTree.setAdapter(lstPestsPerTreeAdapter);
    }

    public void addStop(View v){
        Intent intent=new Intent(this,enterDataActivity.class);
        Bundle b = new Bundle();
        b.putSerializable(SCOUT_STOP,null);
        intent.putExtras(b);
        startActivityForResult(intent, 0);
        //handle new stop object
    }

    public void updateStop(int position){
        //Enter EnterDataActivity for editing the stop
        Intent intent=new Intent(this,enterDataActivity.class);
        Bundle bundle=new Bundle();
        bundle.putSerializable(SCOUT_STOP,mScoutTrip.getStop(position));
        intent.putExtras(bundle);
        startActivityForResult(intent, 1);
        Log.d(TAG, "Updated");
    }

    @Override
    protected void onActivityResult(int requestCode,int resultCode, Intent intent){
        if (requestCode==0 && resultCode==RESULT_OK){
            Bundle bundle=intent.getExtras();
            mScoutTrip.addStop((ScoutStop)bundle.get(SCOUT_STOP));
            lstStopsAdapter.notifyDataSetChanged();
            lstPestsPerTreeAdapter.notifyDataSetChanged();
        }
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_scout_trip, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }

    private class ScoutStopAdapter extends ArrayAdapter<ScoutStop> {
        public ScoutStopAdapter(ArrayList<ScoutStop> stops) {
            super(getApplication().getApplicationContext(), 0, stops);
        }

        @Override
        public View getView(int position, View convertView, ViewGroup parent) {
            if (convertView == null) {
                convertView = getLayoutInflater()
                        .inflate(R.layout.list_scout_stop, null);
            }
            ScoutStop stop = getItem(position);
            TextView lblBlockName =
                    (TextView)convertView.findViewById(R.id.lblBlockName);
            lblBlockName.setText(stop.getBlockName());
            TextView lblTreeAmount =
                    (TextView)convertView.findViewById(R.id.lblTreeAmount);
            lblTreeAmount.setText(stop.getNumTrees()+"");
            LinearLayout hscrollBugInfo=(LinearLayout)convertView.findViewById(R.id.hscrollBugInfo);
            ImageView img=new ImageView(this.getContext());
            //img.setImageResource(R.drawable.st);
            hscrollBugInfo.removeAllViews();
            img.setImageBitmap(BitmapFactory.decodeResource(getResources(),R.drawable.st));
            img.setLayoutParams(new RelativeLayout.LayoutParams(50,50));
            hscrollBugInfo.addView(img);
            return convertView;
        }
    }

    private class PestsPerTreeAdapter extends ArrayAdapter<ScoutStop> {
        public PestsPerTreeAdapter(ArrayList<ScoutStop> stops){
            super(getApplication().getApplicationContext(),0,stops);
        }

        @Override
        public View getView(int position, View convertView, ViewGroup parent) {
            if (convertView==null) {
                convertView=getLayoutInflater().inflate(R.layout.list_pests_per_tree,null);
            }
            ScoutStop stop = getItem(position);
            TextView lblBlockName=(TextView)convertView.findViewById(R.id.lblBlockName);
            lblBlockName.setText(stop.getBlockName());
            TextView lblPestsPerTree=(TextView)convertView.findViewById(R.id.lblPestsPerTree);
            lblPestsPerTree.setText(stop.getPestsPerTree() + "");
            return convertView;
        }
    }

}
