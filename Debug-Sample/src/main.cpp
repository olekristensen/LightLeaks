/*
 This app runs the projection during the calibration. It creates multiple windows
 and displays a coded image in each window. Each window has its own mask, which
 might not exist on the first run. The server app generates a list of patterns to
 be projected, and when it receives the "start" command it will iterate through them.
 */

#include "ofMain.h"
#include "ofAppNoWindow.h"

// this camera always returns after 500ms
class FakeCamera {
private:
    uint64_t lastTime = 0;
    bool newPhoto = false;
    bool waiting = false;
    bool startRequested = false;
    
public:
    void setup() {
        ofAddListener(ofEvents().update, this, &FakeCamera::updateInternal);
    }
    void updateInternal(ofEventArgs &args) {
        uint64_t curTime = ofGetElapsedTimeMillis();
        uint64_t timeDiff = 500;
        if (waiting) {
            if(curTime > lastTime + timeDiff) {
                newPhoto = true;
                waiting = false;
            }
        }
    }
    void takePhoto(string filename) {
        cout << filename << endl;
        lastTime = ofGetElapsedTimeMillis();
        waiting = true;
    }
    bool isStartRequested() {
        bool prevStartRequested = startRequested;
        startRequested = false;
        return prevStartRequested;
    }
    bool isPhotoNew() {
        bool prevNewPhoto = newPhoto;
        newPhoto = false;
        return prevNewPhoto;
    }
    
    // override the start request using this projection app
    void fakeStart() {
        startRequested = true;
    }
};

class ServerApp : public ofBaseApp {
public:
    bool debug = false;
    bool capturing = false;
    bool needToCapture = false;
    uint64_t bufferTime = 100;
    uint64_t lastCaptureTime = 0;
    string timestamp;
    int pattern = 0;
    vector<tuple<int,int,int>> patterns;
    FakeCamera camera;
    
    void setup() {
        ofLog() << "Running";
        camera.setup();
    }
    void config(ofVec2f box) {
        int xk = ceil(log2(box.x));
        int yk = ceil(log2(box.y));
        
        int axis, level, inverted;
        axis = 0;
        for(level = 0; level < xk; level++) {
            for(int inverted : {0,1}) {
                patterns.emplace_back(make_tuple(axis, level, inverted));
            }
        }
        axis = 1;
        for(level = 0; level < yk; level++) {
            for(int inverted : {0,1}) {
                patterns.emplace_back(make_tuple(axis, level, inverted));
            }
        }
        
        cout << "Bounding box: " << box << endl;
        cout << "level count: " << xk << ", " << yk << endl;
        cout << "List of patterns:" << endl;
        for(auto cur : patterns) {
            tie(axis, level, inverted) = cur;
            cout << "\t" << axis << ", " << level << ", " << inverted << endl;
        }
    }
    bool nextState() {
        pattern++;
        if (pattern == patterns.size()) {
            pattern = 0;
            return false;
        }
        return true;
    }
    void update() {
        uint64_t curTime = ofGetElapsedTimeMillis();
        if(!capturing) {
            if(camera.isStartRequested()) {
                timestamp = ofToString(ofGetHours(),2,'0') + ofToString(ofGetMinutes(),2,'0');
                capturing = true;
                needToCapture = true;
            }
        }
        if(camera.isPhotoNew()) {
            if(nextState()) {
                lastCaptureTime = curTime;
                needToCapture = true;
            } else {
                cout << "Done taking photos. Hurray!" << endl;
                capturing = false;
            }
        }
        if(capturing) {
            if(needToCapture && curTime > bufferTime + lastCaptureTime) {
                string directory = "../../../SharedData/scan-" + timestamp + "/cameraImages/" +
                    (getAxis() == 0 ? "vertical/" : "horizontal/") +
                    (getInverted() == 0 ? "normal/" : "inverse/");
                camera.takePhoto(directory + ofToString(getLevel()) + ".jpg");
                needToCapture = false;
            }
        }
    }
    void keyPressed(int key) {
        if(key == 'd') {
            debug = !debug;
        }
        if(key == 's') {
            camera.fakeStart();
        }
    }
    bool getDebug() {
        return debug;
    }
    int getAxis() {
        return get<0>(patterns[pattern]);
    }
    int getLevel() {
        return get<1>(patterns[pattern]);
    }
    int getInverted() {
        return get<2>(patterns[pattern]);
    }
};

class ClientApp : public ofBaseApp {
public:
    int id, xcode, ycode;
    float hue;
    shared_ptr<ServerApp> server;
    
    ofShader shader;
    ofImage mask;
    
    void config(int id, int n, int xcode, int ycode, shared_ptr<ServerApp> server) {
        this->id = id;
        this->hue = id / float(n);
        this->xcode = xcode;
        this->ycode = ycode;
        this->server = server;
        
        string maskFn = "mask-" + ofToString(id) + ".png";
        if(ofFile::doesFileExist(maskFn)) {
            if(mask.load(maskFn)) {
                cout << "Loaded mask: " << maskFn << endl;
            } else {
                cout << "Error loading mask: " << maskFn << endl;
            }
        } else {
            cout << "Mask does not exist: " << maskFn << endl;
        }
    }
    
    void setup() {
        ofBackground(0);
        ofSetVerticalSync(true);
        ofHideCursor();
        ofDisableAntiAliasing();
        shader.load("shader");
    }
    void update() {
    }
    void draw() {
        shader.begin();
        shader.setUniform1i("height", ofGetHeight());
        shader.setUniform1i("axis", server->getAxis());
        shader.setUniform1i("level", server->getLevel());
        shader.setUniform1i("inverted", server->getInverted());
        shader.setUniform1i("xcode", xcode);
        shader.setUniform1i("ycode", ycode);
        ofDrawRectangle(0, 0, ofGetWidth(), ofGetHeight());
        shader.end();
        
        ofPushStyle();
        if(server->getDebug()) {
            ofSetColor(ofColor_<float>::fromHsb(hue, 1, 1));
            ofDrawRectangle(0, 0, ofGetWidth(), ofGetHeight());
            string fps = ofToString(int(round(ofGetFrameRate())));
            ofDrawBitmapStringHighlight(ofToString(id) + "/" + fps, 10, 20);
        } else {
            ofEnableBlendMode(OF_BLENDMODE_MULTIPLY);
            mask.draw(0, 0);
        }
        ofPopStyle();
    }
    void keyPressed(int key) {
        server->keyPressed(key);
    }
};

ofVec2f getBoundingBox(const ofJson& projectors) {
    ofVec2f box;
    for(auto projector : projectors) {
        int x = int(projector["xcode"]) + int(projector["width"]);
        int y = int(projector["ycode"]) + int(projector["height"]);
        box.x = MAX(box.x, x);
        box.y = MAX(box.y, y);
    }
    return box;
}

int main() {
    ofJson config = ofLoadJson("../../../SharedData/settings.json");
    
    shared_ptr<ofAppNoWindow> winServer(new ofAppNoWindow);
    shared_ptr<ServerApp> appServer(new ServerApp);
    ofVec2f box = getBoundingBox(config["projectors"]);
    appServer->config(box);
    ofRunApp(winServer, appServer);
    
    ofGLFWWindowSettings settings;
    settings.setGLVersion(3,2);
    settings.decorated = false;
    settings.numSamples = 1;
    settings.resizable = true;
    
    int n = config["projectors"].size();
    for(int i = 0; i < n; i++) {
        ofJson curConfig = config["projectors"][i];
        settings.monitor = curConfig["monitor"];
        settings.width = curConfig["width"];
        settings.height = curConfig["height"];
        settings.setPosition(ofVec2f(curConfig["xwindow"], curConfig["ywindow"]));
        shared_ptr<ofAppBaseWindow> winClient = ofCreateWindow(settings);
        shared_ptr<ClientApp> appClient(new ClientApp);
        appClient->config(i, n, curConfig["xcode"], curConfig["ycode"], appServer);
        ofRunApp(winClient, appClient);
    }
    
    ofRunMainLoop();
}