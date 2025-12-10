use crate::events::Event;
use crate::store::EventStore;
use crate::{models, projector};

pub mod autobreak;
pub mod config;
pub mod edit;
pub mod favorites;
pub mod push;
pub mod start;
pub mod sync;
pub mod timebank;
pub mod utils;
pub mod comment;

pub fn add_event(
    tasks: &mut Vec<models::Task>,
    history: &mut Vec<Event>,
    event_store: &EventStore,
    event: Event,
    success_msg: &str,
) -> String {
    event_store.persist(&event);
    history.push(event);
    *tasks = projector::restore_state(history);
    success_msg.to_string()
}
