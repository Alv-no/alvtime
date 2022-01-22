import { model, Schema } from "mongoose";
import { logger } from "../createLogger";

type Term = {
  term: string;
  count?: number;
};
export interface Tag extends Term {
  matching_categories?: Term[];
}

const Term = { term: String, count: Number };

const tagSchema = new Schema<Tag>({
  ...Term,
  matching_categories: [Term],
});

const TagModel = model<Tag>("Tags", tagSchema);

export default createTagsDB();
export function createTagsDB(model = TagModel) {
  return {
    async getAll(): Promise<Tag[]> {
      const query = await model.find({}).exec();
      if (!query) return undefined;
      return query;
    },

    async findTag(tagName: string): Promise<Tag> {
      const doc = await model.findById(tagName).exec();
      if (!doc) return undefined;
    },

    save(tag: Tag): Promise<Tag> {
      const doc = new model(tag);
      return doc.save();
    },

    batchInsert(tags: Tag[]) {
      return model.insertMany(tags);
    },

    count() {
      return model.countDocuments();
    },

    exists(term: string) {
      return model.exists({ term });
    },
  };
}
