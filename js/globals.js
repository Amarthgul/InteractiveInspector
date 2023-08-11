
const testInLocal = true; 

const developerMode = true; 

// ---------------------------------------------------------------------
// -------------------------- Class definition -----------------------
// ---------------------------------------------------------------------
// For ease of use later in the scripts. 


class textureSet {
    constructor(diffuse, normal, roughness, metallic) {
        this.diffuse = diffuse;
        this.normal = normal;
        this.roughness = roughness;
        this.metallic = metallic; 
    }
}

// ---------------------------------------------------------------------
// -------------------------- Explicit variables -----------------------
// ---------------------------------------------------------------------
// Variables that may be used externally when adding the export keyword. 


// Path of the root folder in the server 
const rootPath = "https://odeedelessons.org.ohio-state.edu/U.OSU.EDU/ARVR/WebglEmbed"; 

const modelSubFolder = '/public/models/'; 

const textureMapSubFodler = '/public/textures/';

export const MonkeySkullMaps = new textureSet(
    rootPath + textureMapSubFodler + 'SkullDiffuse.jpg', 
    rootPath + textureMapSubFodler + 'SkullNormal.jpg',
    null, null, null
);



// ---------------------------------------------------------------------
// -------------------------- Implicit variables -----------------------
// ---------------------------------------------------------------------
// Variables that should not have the export keyword and should only 
// be used within this file for storage or other purposes. 

// const _PartsNamesWhatever 

const _PartsNamesMonkeySkull = [
    'PartBase.obj',
    'PartFrontal.obj',
    'PartNasal.obj',
    'PartParitalLeft.obj',
    'PartParitalRight.obj'
];

const _PartsRockScene00 = [
    'RockScene_01.fbx'
];

const _PartsRockScene01 = [
    'RockScene_P00.obj', 
    'RockScene_P01.obj'
];

const _MapsRockScene01 = [
    'HughesBluff_Pano_JS.JPG',
    'wd3cciw_2K_Albedo.jpg', 
    'wd3cciw_2K_Normal_LOD0.jpg', 
    'wd3cciw_2K_Roughness.jpg'
]; 

// ---------------------------------------------------------------------
// ----------------------------- Functions -----------------------------
// ---------------------------------------------------------------------


export function getMonkeySkullPartsPaths() {
    // Return the path to every parts of the monkey skull. 
    let pathList = [];
    for (let name of _PartsNamesMonkeySkull) {
        if (testInLocal)
            pathList.push('.' + modelSubFolder + name);
        else
            pathList.push(rootPath + modelSubFolder + name);
    }
    return pathList; 
}

export function getRockScene00PartsPaths() {
    // Return the path to the rock scene (which has only 1 part)
    if (testInLocal)
        return ['.' + modelSubFolder + _PartsRockScene00];
    else
        return [rootPath + modelSubFolder + _PartsRockScene00];
}

export function getRockScene01PartsPaths() {
    // Return the path to the rock scene that consists of 2 parts 
    let pathList = [];
    for (let name of _PartsRockScene01) {
        if (testInLocal)
            pathList.push('.' + modelSubFolder + name);
        else
            pathList.push(rootPath + modelSubFolder + name);
    }
    return pathList; 
}

export function getRockScen01MapsPaths() {
    let pathList = [];
    for (let name of _MapsRockScene01) {
        if (testInLocal)
            pathList.push('.' + textureMapSubFodler + name);
        else
            pathList.push(rootPath + textureMapSubFodler + name);
    }
    return pathList; 
}

// ---------------------------------------------------------------------
// ------------------------------- MISC --------------------------------

export function std135Aov(focalLength) {
    // Given a focal length in mm, convert focal length into angle of view,
    // assuming the lens is mounted on an standard 135 format still camera. 
    return Math.atan(18 / focalLength) * 2 * (180 / Math.PI);
}

