package org.jazor.jolt.rider;

import com.intellij.openapi.fileTypes.FileType;
import javax.swing.Icon;

public final class JoltFileType implements FileType {
    public static final JoltFileType INSTANCE = new JoltFileType();

    private JoltFileType() {}

    @Override public String getName() { return "JOLT"; }
    @Override public String getDescription() { return "Jolt component"; }
    @Override public String getDefaultExtension() { return "jazor"; }
    @Override public Icon getIcon() { return null; }
    @Override public boolean isBinary() { return false; }
}
