import mongoose from "mongoose";
import mongooseFieldEncryption from "mongoose-field-encryption";
import env from "../environment";

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

userSchema.plugin(mongooseFieldEncryption.fieldEncryption, {
  fields: ["auth"],
  secret: env.DB_ENCRYPTION_KEY,
});

const UserModel = mongoose.model("User", userSchema);

interface Model {
  new (userData: UserData & { _id: string }): { save(): Promise<{}> };
  findOneAndUpdate: (
    query: { _id: string },
    doc: { $set: { auth: Auth; __enc_message: false } }
  ) => { exec: () => Promise<{}> };
  find: (query: {}) => { exec: () => Promise<Document[]> };
  findById: (id: string) => { exec: () => Promise<Document> };
}

interface Document {
  toObject: () => UserData & {
    _id: string;
    __enc_auth_d: string;
    __enc_auth: boolean;
    __v: string;
  };
}

export interface Auth {
  tokenType: string;
  scope: string;
  expiresOn: number;
  expiresIn: number;
  extExpiresIn: number;
  accessToken: string;
  idToken: string;
  refreshToken: string;
}

export interface UserData {
  name: string;
  email: string;
  slackUserName: string;
  slackUserID: string;
  auth: Auth;
}

export default createUserDB(UserModel);
export function createUserDB(model: Model) {
  return {
    async getAll(): Promise<UserData[]> {
      const query = await model.find({}).exec();
      if (!query) return undefined;
      return query.map((doc) => prune(doc));
    },

    async updateUserAuth(slackUserID: string, auth: Auth): Promise<void> {
      try {
        model
          .findOneAndUpdate(
            { _id: slackUserID },
            { $set: { auth, __enc_message: false } }
          )
          .exec();
        console.log("Succesfully updated auth on user.");
      } catch (e) {
        console.error("Unable to replace user data: ", e);
      }
    },

    async findById(slackUserID: string): Promise<UserData> {
      const doc = await model.findById(slackUserID).exec();
      if (!doc) return undefined;
      return prune(doc);
    },

    async save(user: UserData): Promise<boolean> {
      let saved = false;
      try {
        const document: UserData & { _id: string } = {
          _id: user.slackUserID,
          ...user,
        };
        const doc = new model(document);
        await doc.save();
        saved = true;
      } catch (e) {
        console.error("Unable to save user data: ", e);
      }
      return saved;
    },
  };
}

function prune(doc: Document) {
  const obj = doc.toObject();
  delete obj._id;
  delete obj.__enc_auth_d;
  delete obj.__enc_auth;
  delete obj.__v;
  return obj;
}
