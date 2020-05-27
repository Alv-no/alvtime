import mongoose from "mongoose";
import encrypt from "mongoose-encryption";
import env from "../environment";

export interface UserData {
  _id: string;
  name: string;
  email: string;
  slackUserName: string;
  slackUserID: string;
  auth: {
    tokenType: string;
    scope: string;
    expiresOn: string;
    expiresIn: number;
    extExpiresIn: number;
    accessToken: string;
    idToken: string;
    refreshToken: string;
  };
}

const userSchema = new mongoose.Schema({
  _id: String,
  name: String,
  email: String,
  slackUserName: String,
  slackUserID: String,
  auth: {
    tokenType: String,
    scope: String,
    expiresOn: String,
    expiresIn: Number,
    extExpiresIn: Number,
    accessToken: String,
    idToken: String,
    refreshToken: String,
  },
});

userSchema.plugin(encrypt, {
  encryptionKey: env.DB_ENCRYPTION_KEY,
  signingKey: env.DB_SIGNING_KEY,
});

export default mongoose.model("User", userSchema);
