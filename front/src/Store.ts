import AES from 'crypto-js/aes';

const secret = "CIPHERKEY";
const valuesKey = "PlantAuthToken";

type Roles = "Consumer" | "Producer" | "Manager";
interface AuthResponse {
  roles: Roles[];
  token: String;
}


const encrypt = (value: string) => {
  var cipher = AES.encrypt(value, secret);
  return cipher.toString();
};

const decrypt = (cipher: string) => {
  var decipher = AES.decrypt(cipher, secret);
  return decipher.toString(CryptoJS.enc.Utf8);
};

const store = (response: AuthResponse) => {
  let str = encrypt(JSON.stringify(response));
  localStorage.setItem(valuesKey, str);
};

const retrieve = (): AuthResponse => {
  let str = decrypt(localStorage.getItem(valuesKey) ?? '');
  return JSON.parse(str);
};

export { Roles, AuthResponse, store, retrieve };