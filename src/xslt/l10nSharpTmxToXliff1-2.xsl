<?xml version='1.0' ?>
<!-- Convert tmx produced by L10nSharp to XLIFF 1.2, skipping "no longer used" items and preserving "dynamic" flag.
-->
<xsl:stylesheet version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" encoding="utf-8" indent="yes" xslt:indent-amount="3" 
		xmlns:xslt="http://xml.apache.org/xslt" />
	<xsl:template match="/">
		<xliff version="1.2">
			<file source-language="en-US">
				<body>
					<xsl:apply-templates select="tmx/body/tu[not(prop[@type='x-nolongerused'])]"/>
				</body>
			</file>
		</xliff>
	</xsl:template>
	<xsl:template match="tu">
		<trans-unit>
			<xsl:attribute name="id">
				<xsl:value-of select="@tuid"/>
			</xsl:attribute>
			<xsl:if test="prop[@type='x-dynamic']">
				<xsl:attribute name="extype">dynamic</xsl:attribute>
			</xsl:if>
			<xsl:if test="note">
				<note>
					<xsl:value-of select="note"/>
				</note>
			</xsl:if>
			<xsl:apply-templates select="tuv"/>
		</trans-unit>
	</xsl:template>
	<xsl:template match="tuv[@xml:lang='en']">
		<source>
			<xsl:attribute name="xml:lang">
				<xsl:value-of select="@xml:lang"/>
			</xsl:attribute>
			<xsl:value-of select="seg"/>
		</source>
	</xsl:template>
	<xsl:template match="tuv[@xml:lang!='en']">
		<target>
			<xsl:attribute name="xml:lang">
				<xsl:value-of select="@xml:lang"/>
			</xsl:attribute>
			<xsl:value-of select="seg"/>
		</target>
	</xsl:template>
</xsl:stylesheet>
