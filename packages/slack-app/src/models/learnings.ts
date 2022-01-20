import { Schema, model } from "mongoose";
import { logger } from "../createLogger";

type MessageRef = { channel: string; timestamp: string };
export interface Learning {
  createdAt: Date;
  description: string;
  locationOfDetails: string;
  shareMessage?: MessageRef;
  thanksMessage: MessageRef;
  shareability: string;
  slackUserID: string;
  learners: string[];
  tags: string[];
}

const MessageRef = { channel: String, timestamp: String };

const learningSchema = new Schema<Learning>({
  createdAt: Date,
  description: String,
  locationOfDetails: String,
  shareMessage: MessageRef,
  thanksMessage: MessageRef,
  shareability: String,
  slackUserID: String,
  timestamp: String,
  learners: [String],
  tags: [String],
});

const LearningModel = model<Learning>("Learnings", learningSchema);

export default createLearningDB();
export function createLearningDB(model = LearningModel) {
  return {
    async getAll(): Promise<Learning[]> {
      const query = await model.find({}).exec();
      if (!query) return undefined;
      return query;
    },

    async findBySlackUserID(slackUserID: string): Promise<Learning> {
      const doc = await model.findById(slackUserID).exec();
      if (!doc) return undefined;
    },

    async findCreatedAfter(date: Date): Promise<Learning[]> {
      const query = await model.find({ createdAt: { $gte: date } });
      if (!query) return undefined;
      return query;
    },

    async save(learning: Learning): Promise<boolean> {
      let saved = false;
      try {
        const doc = new model(learning);
        await doc.save();
        saved = true;
      } catch (e) {
        logger.error("Unable to save learning data");
        logger.error(e);
      }
      return saved;
    },
  };
}
