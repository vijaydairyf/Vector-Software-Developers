package vsd.co.za.sambugapp;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;
import android.graphics.Matrix;
import android.graphics.drawable.BitmapDrawable;
import android.provider.MediaStore;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.GridLayout;
import android.widget.ImageView;
import android.widget.Toast;

import java.io.ByteArrayOutputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.lang.reflect.Array;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;

import vsd.co.za.sambugapp.DataAccess.DBHelper;
import vsd.co.za.sambugapp.DataAccess.SpeciesDAO;
import vsd.co.za.sambugapp.DomainModels.Species;


public class IdentificationActivity extends AppCompatActivity {
    static final int REQUEST_TAKE_PHOTO = 1;
    public static final String IDENTIFICATION_SPECIES="za.co.vsd.identification_species";
    private ImageView mImageView;
    private Bitmap bitmap;
    private Species currentEntry;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_identification);
        // dispatchTakePictureIntent();
        //mImageView = (ImageView) findViewById(R.id.ivFieldPicture);
        bitmap=BitmapFactory.decodeResource(getResources(),R.drawable.st);
        SpeciesDAO speciesDAO = new SpeciesDAO(getApplicationContext());
        try {
            speciesDAO.open();
            if (speciesDAO.isEmpty()) {
                speciesDAO.loadPresets();
            }

            speciesDAO.close();

        } catch (Exception e) {
            e.printStackTrace();
        }

    }



    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_identification, menu);
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

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        InputStream stream = null;
        if (requestCode == REQUEST_TAKE_PHOTO && resultCode == Activity.RESULT_OK)

            try {
                // recyle unused bitmaps
                if (bitmap != null) {
                    bitmap.recycle();
                }
                stream = getContentResolver().openInputStream(data.getData());
                bitmap = BitmapFactory.decodeStream(stream);
                Log.e("BMap", bitmap.toString());
                //TODO: Rotate the image

                mImageView.setImageBitmap(bitmap);

            } catch (FileNotFoundException e) {
                e.printStackTrace();
            } finally {
                if (stream != null)
                    try {
                        stream.close();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
        }
    }

    private void dispatchTakePictureIntent(){
        Intent takePictureIntent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
        if (takePictureIntent.resolveActivity(getPackageManager()) != null) {
            startActivityForResult(takePictureIntent, REQUEST_TAKE_PHOTO);
        }
    }

    public void speciesSelectionClick(View view) {
        int[] buttonIds = {R.id.coconut_1, R.id.coconut_2, R.id.coconut_3, R.id.coconut_4,
                R.id.green_veg_1, R.id.green_veg_2, R.id.green_veg_3, R.id.green_veg_4,
                R.id.two_spot_1, R.id.two_spot_2, R.id.two_spot_3, R.id.two_spot_4,
                R.id.yellow_edged_1, R.id.yellow_edged_2, R.id.yellow_edged_3, R.id.yellow_edged_4};

        for (int id = 0; id < buttonIds.length; id++) {
            if (view.getId() == buttonIds[id]) {
                SpeciesDAO speciesDAO = new SpeciesDAO(getApplicationContext());
                try {
                    speciesDAO.open();
                    currentEntry = speciesDAO.getSpecies(id + 1);
                    Toast.makeText(getApplicationContext(), "You chose " + currentEntry.getSpeciesName() + " at instar " + currentEntry.getLifestage(), Toast.LENGTH_SHORT).show();
                    speciesDAO.close();
                    return;
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        }


    }

    public void sendResultBack(View view) {

        Intent output = new Intent();
        Bundle b = new Bundle();
        currentEntry.setIdealPicture(null);
        b.putSerializable(IDENTIFICATION_SPECIES, currentEntry);
        //Bitmap cp      = mImageView.getDrawingCache();
        Bitmap cp=bitmap;
        cp=Bitmap.createScaledBitmap(cp,50,50,true);
        if(cp == null){
            Log.e("Look", "Bitch");
        }
        b.putParcelable("Image",cp);
        output.putExtras(b);
        //output.putExtra("Image",cp);
        setResult(RESULT_OK, output);
        finish();

    }



}
