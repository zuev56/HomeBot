-- (!) Script must be reexecutable

CREATE INDEX IF NOT EXISTS "IX_activity_log_user_id_last_seen_insert_date"
    ON vk.activity_log USING btree
    (user_id, last_seen, insert_date);